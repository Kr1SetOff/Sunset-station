// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Arena;

/// <summary>
/// Client -> server. A ghost asks to create a new arena match in the given mode.
/// Only honored while the arena is <see cref="ArenaState.Idle"/>.
/// </summary>
[Serializable, NetSerializable]
public sealed class ArenaCreateRequestEvent : EntityEventArgs
{
    public ArenaMode Mode;

    public ArenaCreateRequestEvent(ArenaMode mode)
    {
        Mode = mode;
    }
}

/// <summary>
/// Client -> server. A ghost asks to join the currently queueing match.
/// </summary>
[Serializable, NetSerializable]
public sealed class ArenaJoinRequestEvent : EntityEventArgs
{
}

/// <summary>
/// Client -> server. A ghost asks to be teleported to the arena to spectate the ongoing fight.
/// </summary>
[Serializable, NetSerializable]
public sealed class ArenaSpectateRequestEvent : EntityEventArgs
{
}

/// <summary>
/// Server -> all clients. Broadcasts the current arena state so ghost UIs can update.
/// </summary>
[Serializable, NetSerializable]
public sealed class ArenaStatusEvent : EntityEventArgs
{
    public ArenaState State;
    public ArenaMode Mode;

    /// <summary>How many ghosts are currently queued (while queueing) or fighting.</summary>
    public int Participants;

    /// <summary>Seconds left on the queue timer (only meaningful while queueing).</summary>
    public float TimeLeft;

    /// <summary>Whether the receiving player is currently in the queue. Set per-recipient.</summary>
    public bool InQueue;

    public ArenaStatusEvent(ArenaState state, ArenaMode mode, int participants, float timeLeft, bool inQueue)
    {
        State = state;
        Mode = mode;
        Participants = participants;
        TimeLeft = timeLeft;
        InQueue = inQueue;
    }
}
