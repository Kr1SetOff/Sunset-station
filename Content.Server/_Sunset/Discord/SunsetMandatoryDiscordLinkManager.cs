using Content.Server._Sunset.SponsorTier;
using Content.Server.EUI;
using Content.Shared._Sunset.CCVar;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Enums;
using Robust.Shared.Player;

namespace Content.Server._Sunset.Discord;

/// <summary>
/// Enforces mandatory Discord linking: as soon as a player without a linked Discord account
/// joins, the link window opens and (client-side) refuses to close until the link completes.
/// Already-linked players are never prompted - the link persists in the DB, so this only ever
/// fires once per account. Does nothing while OAuth is unconfigured or the CVar is off.
/// </summary>
public sealed class SunsetMandatoryDiscordLinkManager : IPostInjectInit
{
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private EuiManager _euiManager = default!;
    [Dependency] private SunsetDiscordOAuth _oauth = default!;
    [Dependency] private SunsetSponsorTierService _tierService = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private ILogManager _logManager = default!;

    private readonly Dictionary<ICommonSession, SunsetDiscordLinkEui> _openPrompts = new();
    private ISawmill _sawmill = default!;

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("sunset.discord_required");
        _player.PlayerStatusChanged += OnPlayerStatusChanged;
        _tierService.PlayerLinked += OnPlayerLinked;
    }

    private void OnPlayerStatusChanged(object? sender, SessionStatusEventArgs e)
    {
        switch (e.NewStatus)
        {
            case SessionStatus.InGame:
                MaybePrompt(e.Session);
                break;
            case SessionStatus.Disconnected:
                _openPrompts.Remove(e.Session);
                break;
        }
    }

    private void MaybePrompt(ICommonSession session)
    {
        if (!_cfg.GetCVar(SunsetCCVars.SunsetDiscordLinkRequired))
            return;

        if (_tierService.IsLinked(session))
            return;

        // Unconfigured OAuth would make the link impossible - don't trap players behind
        // a window whose "link" button can never work.
        if (string.IsNullOrEmpty(_oauth.GetAuthUrl(session.UserId)))
        {
            _sawmill.Warning("Discord link is required but OAuth is unconfigured; skipping mandatory prompt.");
            return;
        }

        if (_openPrompts.ContainsKey(session))
            return;

        var eui = new SunsetDiscordLinkEui(mandatory: true);
        _euiManager.OpenEui(eui, session);
        eui.StateDirty();
        _openPrompts[session] = eui;
    }

    private void OnPlayerLinked(ICommonSession session)
    {
        // Refresh the open prompt so the client sees IsLinked = true and unblocks closing.
        if (_openPrompts.Remove(session, out var eui))
            eui.StateDirty();
    }
}
