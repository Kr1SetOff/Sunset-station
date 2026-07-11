using System.Threading.Tasks;
using Content.Client.Players.PlayTimeTracking;
using Content.Shared._Sunset.SponsorTier;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Client._Sunset.SponsorTier;

/// <summary>
/// The client only knows the local player's own tier, synced down via MsgJobWhitelist.SponsorTier
/// (see JobRequirementsManager) - it has no visibility into other sessions' tiers, since those are
/// never sent down. Good enough for gating the local player's own UI (e.g. loadout requirements);
/// actual access control is still enforced server-side regardless of what this returns.
/// </summary>
public sealed class ClientSunsetSponsorTierReader : ISunsetSponsorTierReader
{
    [Dependency] private JobRequirementsManager _jobRequirements = default!;

    public int GetSponsorTier(NetUserId userId) => _jobRequirements.SponsorTier;

    public int GetSponsorTier(ICommonSession session) => _jobRequirements.SponsorTier;

    public bool TryGetSponsorTier(ICommonSession session, out int tier)
    {
        tier = _jobRequirements.SponsorTier;
        return tier > 0;
    }

    public Task<int> GetSponsorTierAsync(NetUserId userId) => Task.FromResult(_jobRequirements.SponsorTier);
}
