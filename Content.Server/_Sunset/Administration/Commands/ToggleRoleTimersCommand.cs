using Content.Server.Administration;
using Content.Server.Administration.Commands;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Console;

namespace Content.Server._Sunset.Administration.Commands;

/// <summary>
/// 🌇Sunset🌇 - toggles the game.role_timers cvar (playtime requirements for jobs/antags) at
/// runtime, so admins can open up every role without editing the server config. Wired to a button
/// in the admin menu's Round tab; reuses PanicBunkerCommand's toggle helper (no args = flip,
/// or explicit true/false).
/// </summary>
[AdminCommand(AdminFlags.Server)]
public sealed partial class ToggleRoleTimersCommand : LocalizedCommands
{
    [Dependency] private IConfigurationManager _cfg = default!;

    public override string Command => "toggleroletimers";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        var toggle = PanicBunkerCommand.Toggle(CCVars.GameRoleTimers, shell, args, _cfg, LocalizationManager);
        if (toggle == null)
            return;

        shell.WriteLine(Loc.GetString(toggle.Value ? "cmd-toggleroletimers-enabled" : "cmd-toggleroletimers-disabled"));
    }
}
