using Content.Server._Sunset.SponsorTier;
using Content.Server.EUI;
using Content.Shared._Sunset.Discord;
using Content.Shared.Eui;
using Robust.Shared.IoC;

namespace Content.Server._Sunset.Discord;

/// <summary>
/// Backs the "Link Discord" window opened via the <c>linkdiscord</c> command (itself triggered by the
/// existing Escape-menu/info-banner "Connect Discord" buttons). Shows the OAuth URL plus current link/tier status.
/// </summary>
public sealed class SunsetDiscordLinkEui : BaseEui
{
    private readonly SunsetDiscordOAuth _oauth;
    private readonly SunsetSponsorTierService _tierService;

    public SunsetDiscordLinkEui()
    {
        _oauth = IoCManager.Resolve<SunsetDiscordOAuth>();
        _tierService = IoCManager.Resolve<SunsetSponsorTierService>();
    }

    public override SunsetDiscordLinkEuiState GetNewState() => new()
    {
        Url = _oauth.GetAuthUrl(Player.UserId),
        IsLinked = _tierService.IsLinked(Player),
        Tier = _tierService.GetSponsorTier(Player),
    };

    public override void HandleMessage(EuiMessageBase msg)
    {
        base.HandleMessage(msg);

        if (msg is SunsetDiscordRefreshRequestMessage)
        {
            RefreshAndUpdate();
        }
    }

    private async void RefreshAndUpdate()
    {
        await _tierService.RefreshAsync(Player.UserId);
        StateDirty();
    }
}
