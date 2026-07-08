// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Map;

namespace Content.Server._Sunset.Arena.Components;

/// <summary>
/// Placed on the arena map entity. Caches the spawn points and the spectator/center coordinates
/// so <see cref="ArenaSystem"/> does not have to recompute them.
/// </summary>
[RegisterComponent]
public sealed partial class ArenaMapComponent : Component
{
    /// <summary>Fighter spawn points on this arena.</summary>
    [DataField]
    public List<EntityCoordinates> SpawnPoints = new();

    /// <summary>Where spectators are teleported to.</summary>
    [DataField]
    public EntityCoordinates Center;
}
