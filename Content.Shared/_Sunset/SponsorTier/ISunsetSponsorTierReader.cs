using System.Threading.Tasks;
using Robust.Shared.Network;
using Robust.Shared.Player;

namespace Content.Shared._Sunset.SponsorTier;

/// <summary>
/// Read-only access to a player's resolved Boosty sponsor tier. Declared in Shared so Shared-side gating
/// code (e.g. ghost theme requirements) can depend on it without referencing Content.Server directly -
/// the real implementation (<c>SunsetSponsorTierService</c>) is registered server-side only.
/// </summary>
public interface ISunsetSponsorTierReader
{
    /// <summary>
    /// Synchronous, in-memory lookup. Returns 0 (None) if the player isn't linked, isn't a sponsor, or isn't cached yet.
    /// </summary>
    int GetSponsorTier(NetUserId userId);

    int GetSponsorTier(ICommonSession session);

    bool TryGetSponsorTier(ICommonSession session, out int tier);

    /// <summary>
    /// DB-backed lookup for players who aren't connected yet (e.g. the soft player-cap check during connection).
    /// </summary>
    Task<int> GetSponsorTierAsync(NetUserId userId);
}
