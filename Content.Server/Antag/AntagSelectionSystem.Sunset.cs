using System.Linq;
using Content.Server._Sunset.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking;
using Content.Shared._Sunset.CCVar;
using Content.Shared._Sunset.SponsorTier;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Players;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Antag;

// 🌇Sunset🌇 - Boosty sponsor tier 5's "99% guaranteed antagonist" perk. Kept in its own partial file
// (matching this repo's convention of splitting manager/system classes by feature, e.g. NullLinkPlayerManager)
// since it's a bolt-on perk, not core antag-selection logic.
public sealed partial class AntagSelectionSystem
{
    [Dependency] private ISunsetSponsorTierReader _sunsetSponsorTiers = default!;
    [Dependency] private IConfigurationManager _sunsetCfg = default!;

    private int _sunsetTier5ForcedThisRound;

    private void InitializeSunset()
    {
        // NOTE: does NOT subscribe to RulePlayerJobsAssignedEvent here - AntagSelectionSystem already
        // subscribes once in its main Initialize(), and RobustToolbox disallows the same subscriber
        // registering twice for the same event type. OnSunsetJobsAssigned is instead called directly
        // from the end of OnJobsAssigned (see AntagSelectionSystem.cs).
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnSunsetRoundRestart);
    }

    private void OnSunsetRoundRestart(RoundRestartCleanupEvent args)
    {
        _sunsetTier5ForcedThisRound = 0;
    }

    private void OnSunsetJobsAssigned(RulePlayerJobsAssignedEvent args)
    {
        var maxPerRound = _sunsetCfg.GetCVar(SunsetCCVars.SunsetTier5MaxForcedAntagsPerRound);

        foreach (var session in args.Players)
        {
            if (_sunsetTier5ForcedThisRound >= maxPerRound)
                break;

            if (_sunsetSponsorTiers.GetSponsorTier(session) < 5)
                continue;

            if (_role.MindIsAntagonist(session.GetMind()))
                continue; // already an antagonist through normal selection - no need to force anything.

            if (!RobustRandom.Prob(0.99f))
                continue;

            if (TryForceSunsetTier5Antag(session))
                _sunsetTier5ForcedThisRound++;
        }
    }

    /// <summary>
    /// Tries to force-assign a tier-5 sponsor into any currently-active, player-pickable antag definition,
    /// bypassing the normal preference/pool lottery (but not the underlying eligibility checks in
    /// <see cref="TryMakeAntag"/>, e.g. whitelist/blacklist/already-antag exclusions still apply).
    /// </summary>
    private bool TryForceSunsetTier5Antag(ICommonSession session)
    {
        var candidates = new List<(EntityUid Rule, AntagSelectionComponent Comp, AntagSelectionDefinition Def)>();

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var antag, out _))
        {
            foreach (var def in antag.Definitions)
            {
                if (def.PickPlayer)
                    candidates.Add((uid, antag, def));
            }
        }

        RobustRandom.Shuffle(candidates);

        foreach (var (uid, antag, def) in candidates)
        {
            if (!TryMakeAntag((uid, antag), session, def, checkPref: false))
                continue;

            _adminLogger.Add(LogType.AntagSelection,
                $"Sunset tier-5 guarantee force-assigned {session} as antagonist: {ToPrettyString(uid)}");
            return true;
        }

        return false;
    }
}
