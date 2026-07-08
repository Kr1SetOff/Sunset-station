// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Sunset.Arena;

/// <summary>
/// The three combat modes a ghost arena fight can run in.
/// </summary>
public enum ArenaMode : byte
{
    /// <summary>Each fighter gets 1-3 random melee weapons.</summary>
    Melee,

    /// <summary>Each fighter gets one gun and one melee weapon.</summary>
    MeleeRanged,

    /// <summary>Each fighter gets two guns.</summary>
    Ranged,
}

/// <summary>
/// Current state of the global arena match.
/// </summary>
public enum ArenaState : byte
{
    /// <summary>No match. A ghost may create one.</summary>
    Idle,

    /// <summary>A match was created and ghosts are queueing up before the fight starts.</summary>
    Queueing,

    /// <summary>The fight is in progress on the arena map.</summary>
    Fighting,

    /// <summary>The fight ended and the arena is being cleaned up. New matches are blocked.</summary>
    Cooldown,
}
