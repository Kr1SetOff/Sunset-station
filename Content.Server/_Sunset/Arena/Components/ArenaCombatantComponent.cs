// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Network;

namespace Content.Server._Sunset.Arena.Components;

/// <summary>
/// Marks a temporary body spawned for an arena fighter. When this entity dies or the match ends,
/// <see cref="ArenaSystem"/> returns the player to an observer ghost and deletes the body.
/// </summary>
[RegisterComponent]
public sealed partial class ArenaCombatantComponent : Component
{
    /// <summary>
    /// The user controlling this temporary body, so we can return them to a ghost on death.
    /// </summary>
    [DataField]
    public NetUserId User;

    /// <summary>
    /// The entity the player owned before entering the arena (their round body/corpse, or their
    /// observer ghost). When the arena body dies, the player is returned to it so they keep the
    /// ability to return to their real body. Null if they had no body.
    /// </summary>
    [DataField]
    public NetEntity? OriginalBody;
}
