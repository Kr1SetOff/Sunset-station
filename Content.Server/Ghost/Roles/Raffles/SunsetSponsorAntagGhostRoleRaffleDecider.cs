using System.Linq;
using Content.Shared._Sunset.SponsorTier;
using JetBrains.Annotations;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Ghost.Roles.Raffles;

/// <summary>
/// 🌇Sunset🌇 - chooses the winner of a ghost role raffle with sponsors getting better odds. Only
/// wired in for ghost roles that actually hand out an antagonist mind role (see
/// GhostRoleSystem.Sunset.cs) - non-antag ghost roles keep using the plain
/// <see cref="RngGhostRoleRaffleDecider"/>, so this doesn't affect who gets to be a random cow.
/// The per-tier boost is data-driven (<see cref="ExtraWeightByTier"/>), so specific roles can
/// declare their own decider prototype with different odds (e.g. the blood worm's
/// "sunsetBloodWormRaffle", where tier 2 gets exactly +50%).
/// Tier 5 mirrors the round-start 99% guarantee (see AntagSelectionSystem.Sunset.cs): if any tier-5
/// sponsor is in the raffle, one of them wins outright unless the 1% roll misses, in which case
/// everyone (including that tier-5 sponsor) falls back to the weighted lottery below.
/// </summary>
[UsedImplicitly(ImplicitUseTargetFlags.WithMembers)]
public sealed partial class SunsetSponsorAntagGhostRoleRaffleDecider : IGhostRoleRaffleDecider
{
    /// <summary>
    /// Extra selection weight per sponsor tier, on top of everyone's base weight of 1. E.g. 0.5
    /// means "+50% chance relative to a non-sponsor", 6 means "7x a non-sponsor's chance". Tiers
    /// not listed get no boost. Defaults mirror the round-start pool multipliers in
    /// SunsetAntagWeighting (tiers 1-4: 4/6/8/6).
    /// </summary>
    [DataField]
    public Dictionary<int, float> ExtraWeightByTier = new()
    {
        { 1, 4f },
        { 2, 6f },
        { 3, 8f },
        { 4, 6f },
    };

    /// <summary>
    /// Probability that a tier-5 sponsor in the raffle just wins it outright.
    /// </summary>
    [DataField]
    public float Tier5WinProb = 0.99f;

    public void PickWinner(IEnumerable<ICommonSession> candidates, Func<ICommonSession, bool> tryTakeover)
    {
        var random = IoCManager.Resolve<IRobustRandom>();
        var sponsorTiers = IoCManager.Resolve<ISunsetSponsorTierReader>();

        var pool = candidates.ToList();
        random.Shuffle(pool);

        var tier5 = pool.Where(session => sponsorTiers.GetSponsorTier(session) >= 5).ToList();
        if (tier5.Count > 0 && random.Prob(Tier5WinProb))
        {
            var winner = tier5[random.Next(tier5.Count)];
            if (tryTakeover(winner))
                return;

            pool.Remove(winner);
        }

        // Weighted lottery with fractional weights: everyone gets base weight 1, sponsors get
        // their tier's extra on top. Losers of a failed takeover are removed and the roll repeats.
        var weighted = new List<(ICommonSession Session, float Weight)>();
        foreach (var session in pool)
        {
            ExtraWeightByTier.TryGetValue(sponsorTiers.GetSponsorTier(session), out var extra);
            weighted.Add((session, 1f + MathF.Max(0f, extra)));
        }

        while (weighted.Count > 0)
        {
            var total = 0f;
            foreach (var entry in weighted)
                total += entry.Weight;

            var roll = random.NextFloat() * total;
            var index = weighted.Count - 1;
            var accumulated = 0f;

            for (var i = 0; i < weighted.Count; i++)
            {
                accumulated += weighted[i].Weight;
                if (roll <= accumulated)
                {
                    index = i;
                    break;
                }
            }

            if (tryTakeover(weighted[index].Session))
                return;

            weighted.RemoveAt(index);
        }
    }
}
