// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Arena;

namespace Content.Client._Sunset.Arena;

/// <summary>
/// Client-side companion for the ghost arena. Tracks the latest broadcast status and exposes helpers
/// the arena window uses to send requests to the server.
/// </summary>
public sealed class ArenaSystem : EntitySystem
{
    /// <summary>Latest status received from the server, if any.</summary>
    public ArenaStatusEvent? LastStatus { get; private set; }

    /// <summary>Raised whenever a new status arrives from the server.</summary>
    public event Action<ArenaStatusEvent>? StatusChanged;

    /// <summary>Raised whenever a requested leaderboard page arrives from the server.</summary>
    public event Action<ArenaLeaderboardResponseEvent>? LeaderboardReceived;

    public override void Initialize()
    {
        SubscribeNetworkEvent<ArenaStatusEvent>(OnStatus);
        SubscribeNetworkEvent<ArenaLeaderboardResponseEvent>(OnLeaderboard); // 🌇Sunset🌇
    }

    private void OnStatus(ArenaStatusEvent ev)
    {
        LastStatus = ev;
        StatusChanged?.Invoke(ev);
    }

    private void OnLeaderboard(ArenaLeaderboardResponseEvent ev)
    {
        LeaderboardReceived?.Invoke(ev);
    }

    public void Create(ArenaMode mode)
    {
        RaiseNetworkEvent(new ArenaCreateRequestEvent(mode));
    }

    public void Join()
    {
        RaiseNetworkEvent(new ArenaJoinRequestEvent());
    }

    public void Spectate()
    {
        RaiseNetworkEvent(new ArenaSpectateRequestEvent());
    }

    public void RequestLeaderboard(ArenaMode mode)
    {
        RaiseNetworkEvent(new ArenaLeaderboardRequestEvent(mode));
    }
}
