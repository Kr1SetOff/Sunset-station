using System.Threading.Tasks;
using Content.Shared._Sunset.SponsorTier;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Client._Sunset.SponsorTier;

/// <summary>
/// The client has no access to sponsor tier data - it's resolved server-side from the Discord-linked DB
/// row and never sent down. This stub exists purely so Shared code that depends on
/// <see cref="ISunsetSponsorTierReader"/> (e.g. SponsorTierRequirement.GetRequirementDescription, used by
/// the ghost theme picker to render a tooltip) can be constructed on the client without an
/// unregistered-dependency crash. Actual access gating still happens server-side.
/// </summary>
public sealed class ClientSunsetSponsorTierReader : ISunsetSponsorTierReader
{
    public int GetSponsorTier(NetUserId userId) => 0;

    public int GetSponsorTier(ICommonSession session) => 0;

    public bool TryGetSponsorTier(ICommonSession session, out int tier)
    {
        tier = 0;
        return false;
    }

    public Task<int> GetSponsorTierAsync(NetUserId userId) => Task.FromResult(0);
}
