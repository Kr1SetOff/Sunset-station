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

/// <summary>
/// Client -> server. Asks for the persisted top-players leaderboard for one arena mode.
/// </summary>
[Serializable, NetSerializable]
public sealed class ArenaLeaderboardRequestEvent : EntityEventArgs
{
    public ArenaMode Mode;

    public ArenaLeaderboardRequestEvent(ArenaMode mode)
    {
        Mode = mode;
    }
}

/// <summary>
/// Server -> requester. The top players for the mode that was asked about, already sorted by wins
/// (kills as a tiebreaker) - see IServerDbManager.GetTopArenaPlayers.
/// </summary>
[Serializable, NetSerializable]
public sealed class ArenaLeaderboardResponseEvent : EntityEventArgs
{
    public ArenaMode Mode;
    public List<ArenaLeaderboardEntry> Entries;

    public ArenaLeaderboardResponseEvent(ArenaMode mode, List<ArenaLeaderboardEntry> entries)
    {
        Mode = mode;
        Entries = entries;
    }
}

[Serializable, NetSerializable]
public sealed class ArenaLeaderboardEntry
{
    public string Name;
    public int Kills;
    public int Deaths;
    public int Wins;

    public ArenaLeaderboardEntry(string name, int kills, int deaths, int wins)
    {
        Name = name;
        Kills = kills;
        Deaths = deaths;
        Wins = wins;
    }
}
