using Content.Server.EUI;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server._Sunset.Discord;

/// <summary>
/// Opens the Discord account-linking window. Triggered by the existing "Connect Discord" buttons
/// (Escape menu, server info banner) via <c>RemoteExecuteCommand</c>, matching the same pattern used
/// by the "ghostTheme" command for its EUI.
/// </summary>
[AnyCommand]
public sealed class SunsetLinkDiscordCommand : IConsoleCommand
{
    [Dependency] private EuiManager _euiManager = default!;

    public string Command => "linkdiscord";
    public string Description => "Opens the Discord account linking window.";
    public string Help => Command;

    public void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (shell.Player is not { } player)
        {
            shell.WriteLine("You can only link Discord from a client.");
            return;
        }

        _euiManager.OpenEui(new SunsetDiscordLinkEui(), player);
    }
}
