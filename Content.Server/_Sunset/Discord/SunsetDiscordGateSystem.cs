using Content.Server.EUI;
using Content.Server._Sunset.SponsorTier;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server._Sunset.Discord;

/// <summary>
/// Sunset: forces a freshly-connected, Discord-unlinked player into a mandatory
/// <see cref="SunsetDiscordLinkEui"/> gate the moment they reach the lobby, mirroring how
/// <see cref="Content.Server.Info.RulesManager"/> pushes its rules popup on connect. Previously
/// linking Discord was only reachable through an easy-to-miss Escape-menu/info-banner button and
/// nothing actually enforced it, despite the server rules requiring it.
///
/// No-ops entirely if this deployment hasn't configured Discord OAuth (see
/// <see cref="SunsetDiscordOAuth.IsConfigured"/>), so local/dev servers without OAuth secrets set up
/// don't lock every player out.
/// </summary>
public sealed class SunsetDiscordGateSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly EuiManager _euiManager = default!;
    [Dependency] private readonly SunsetSponsorTierService _tierService = default!;
    [Dependency] private readonly SunsetDiscordOAuth _oauth = default!;

    public override void Initialize()
    {
        base.Initialize();

        _playerManager.PlayerStatusChanged += OnPlayerStatusChanged;
    }

    public override void Shutdown()
    {
        base.Shutdown();

        _playerManager.PlayerStatusChanged -= OnPlayerStatusChanged;
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        if (e.NewStatus != SessionStatus.Connected)
            return;

        if (!_oauth.IsConfigured() || _tierService.IsLinked(e.Session))
            return;

        SunsetDiscordLinkEui.EnsureRequiredOpenFor(_euiManager, e.Session);
    }
}
