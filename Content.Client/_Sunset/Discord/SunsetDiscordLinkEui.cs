using Content.Client.Eui;
using Content.Shared._Sunset.Discord;
using Content.Shared.Eui;
using JetBrains.Annotations;

namespace Content.Client._Sunset.Discord;

[UsedImplicitly]
public sealed class SunsetDiscordLinkEui : BaseEui
{
    private readonly SunsetDiscordLinkWindow _window;

    public SunsetDiscordLinkEui()
    {
        _window = new SunsetDiscordLinkWindow();
        _window.OnRefreshRequested += () => SendMessage(new SunsetDiscordRefreshRequestMessage());
    }

    public override void Opened()
    {
        base.Opened();
        _window.OpenCentered();
    }

    public override void Closed()
    {
        base.Closed();
        _window.Close();
    }

    public override void HandleState(EuiStateBase state)
    {
        base.HandleState(state);
        if (state is SunsetDiscordLinkEuiState linkState)
            _window.UpdateState(linkState);
    }
}
