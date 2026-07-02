using System.Numerics;
using Content.Shared._Sunset.Discord;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;

namespace Content.Client._Sunset.Discord;

public sealed class SunsetDiscordLinkWindow : DefaultWindow
{
    private readonly IUriOpener _uriOpener;
    private readonly Button _linkButton;
    private readonly Button _refreshButton;
    private readonly Label _statusLabel;
    private string _url = "";

    public event Action? OnRefreshRequested;

    public SunsetDiscordLinkWindow()
    {
        _uriOpener = IoCManager.Resolve<IUriOpener>();

        Title = Loc.GetString("sunset-discord-link-title");
        MinSize = new Vector2(440, 0);

        var box = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Vertical,
            Margin = new Thickness(12),
        };

        var text = new RichTextLabel { HorizontalExpand = true };
        text.SetMessage(Loc.GetString("sunset-discord-link-text"));
        box.AddChild(text);

        _statusLabel = new Label
        {
            Margin = new Thickness(0, 8, 0, 0),
            HorizontalAlignment = Control.HAlignment.Center,
        };
        box.AddChild(_statusLabel);

        var buttons = new BoxContainer
        {
            Orientation = BoxContainer.LayoutOrientation.Horizontal,
            HorizontalAlignment = Control.HAlignment.Center,
            Margin = new Thickness(0, 12, 0, 0),
            SeparationOverride = 8,
        };

        _linkButton = new Button { Text = Loc.GetString("sunset-discord-link-button") };
        _linkButton.OnPressed += _ =>
        {
            if (!string.IsNullOrEmpty(_url))
                _uriOpener.OpenUri(_url);
        };
        buttons.AddChild(_linkButton);

        _refreshButton = new Button { Text = Loc.GetString("sunset-discord-link-refresh-button") };
        _refreshButton.OnPressed += _ => OnRefreshRequested?.Invoke();
        buttons.AddChild(_refreshButton);

        box.AddChild(buttons);

        Contents.AddChild(box);
    }

    public void UpdateState(SunsetDiscordLinkEuiState state)
    {
        _url = state.Url;
        _linkButton.Disabled = string.IsNullOrEmpty(_url);

        _statusLabel.Text = state.IsLinked
            ? Loc.GetString("sunset-discord-link-status-linked", ("tier", TierName(state.Tier)))
            : Loc.GetString("sunset-discord-link-status-unlinked");
    }

    private static string TierName(int tier) => tier switch
    {
        1 => Loc.GetString("sunset-sponsor-tier-name-1"),
        2 => Loc.GetString("sunset-sponsor-tier-name-2"),
        3 => Loc.GetString("sunset-sponsor-tier-name-3"),
        4 => Loc.GetString("sunset-sponsor-tier-name-4"),
        5 => Loc.GetString("sunset-sponsor-tier-name-5"),
        _ => Loc.GetString("sunset-sponsor-tier-name-0"),
    };
}
