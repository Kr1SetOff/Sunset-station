using System.Threading;
using Content.Server._Sunset.SponsorTier;
using Content.Server.EUI;
using Content.Shared._Sunset.Discord;
using Content.Shared.Eui;
using Robust.Shared.IoC;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.Discord;

/// <summary>
/// Backs the "Link Discord" window, opened either voluntarily (via the <c>linkdiscord</c> command,
/// itself triggered by the existing Escape-menu/info-banner "Connect Discord" buttons) or as a
/// mandatory join-gate (<see cref="SunsetDiscordGateSystem"/>, <see cref="Required"/> = true) that
/// blocks a freshly-connected, unlinked player from doing anything else in the lobby.
///
/// Sunset: previously this only pushed a state on manual "Refresh status" clicks, so the link button
/// stayed disabled (empty URL) until the player pressed refresh once - now it pushes on open, polls
/// every few seconds while unlinked, and gets nudged instantly by
/// <see cref="SunsetDiscordCallbackHandler"/> the moment the OAuth callback actually links the
/// account - at which point a required gate closes itself automatically, no extra click needed.
/// </summary>
public sealed class SunsetDiscordLinkEui : BaseEui
{
    // Sunset: lets the OAuth callback handler (which only has the NetUserId, not an EUI reference)
    // push an immediate update instead of waiting for the next poll tick.
    private static readonly Dictionary<NetUserId, SunsetDiscordLinkEui> OpenByUser = new();

    private readonly SunsetDiscordOAuth _oauth;
    private readonly SunsetSponsorTierService _tierService;
    private CancellationTokenSource? _pollCancel;

    /// <summary>
    /// True when this window is a mandatory join-gate rather than a voluntary status check - the
    /// client hides its close button while this is set and unlinked, and the server auto-closes it
    /// (instead of just refreshing state) the moment linking succeeds.
    /// </summary>
    public bool Required { get; }

    public SunsetDiscordLinkEui(bool required = false)
    {
        Required = required;
        _oauth = IoCManager.Resolve<SunsetDiscordOAuth>();
        _tierService = IoCManager.Resolve<SunsetSponsorTierService>();
    }

    /// <summary>
    /// Sunset: opens a mandatory gate window for this player, unless one is already open for them.
    /// </summary>
    public static void EnsureRequiredOpenFor(EuiManager euiManager, ICommonSession player)
    {
        if (OpenByUser.ContainsKey(player.UserId))
            return;

        euiManager.OpenEui(new SunsetDiscordLinkEui(required: true), player);
    }

    public override void Opened()
    {
        base.Opened();

        OpenByUser[Player.UserId] = this;

        // Sunset: push the real state immediately instead of leaving the client with the default
        // empty-URL state until it manually asks for a refresh.
        StateDirty();

        _pollCancel = new CancellationTokenSource();
        Robust.Shared.Timing.Timer.SpawnRepeating(TimeSpan.FromSeconds(5), Poll, _pollCancel.Token);
    }

    public override void Closed()
    {
        base.Closed();

        _pollCancel?.Cancel();
        OpenByUser.Remove(Player.UserId);
    }

    public override SunsetDiscordLinkEuiState GetNewState() => new()
    {
        Url = _oauth.GetAuthUrl(Player.UserId),
        IsLinked = _tierService.IsLinked(Player),
        Tier = _tierService.GetSponsorTier(Player),
        Required = Required,
    };

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is SunsetDiscordRefreshRequestMessage)
        {
            RefreshAndUpdate();
        }
    }

    private void Poll()
    {
        if (_tierService.IsLinked(Player))
        {
            OnLinked();
            return;
        }

        RefreshAndUpdate();
    }

    private async void RefreshAndUpdate()
    {
        await _tierService.RefreshAsync(Player.UserId);

        if (_tierService.IsLinked(Player))
            OnLinked();
        else
            StateDirty();
    }

    /// <summary>
    /// Sunset: called by <see cref="SunsetDiscordCallbackHandler"/> right after a successful OAuth
    /// link, so an already-open window updates immediately instead of waiting for the next poll tick.
    /// </summary>
    public static void NotifyLinked(NetUserId userId)
    {
        if (OpenByUser.TryGetValue(userId, out var eui))
            eui.OnLinked();
    }

    private void OnLinked()
    {
        _pollCancel?.Cancel();

        // A required gate has done its job the instant linking succeeds - close it outright so the
        // player lands back in the lobby immediately instead of having to dismiss anything themselves.
        if (Required)
            Close();
        else
            StateDirty();
    }
}
