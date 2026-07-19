using Content.Server.Administration.Systems;
using Content.Shared.Administration;
using Robust.Shared.Console;

namespace Content.Server.Administration.Commands;

// 🌇Sunset🌇 - "Игроки+" admin menu tab backend: pads the player count shown in the launcher
// (server status JSON), without touching the real player count or RobustToolbox. Locked to one
// specific ckey (FakePlayerCountConstants.AllowedCkey), not an admin flag - [AdminCommand(Host)] is
// just an outer "must be some kind of admin" gate, the ckey check below is what actually matters.
[AdminCommand(AdminFlags.Host)]
public sealed partial class FakePlayerCountAddCommand : LocalizedEntityCommands
{
    [Dependency] private AdminSystem _adminSystem = default!;

    public override string Command => "fakeplayercount_add";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!FakePlayerCountCommandShared.CheckAllowed(shell))
            return;

        if (args.Length != 1 || !int.TryParse(args[0], out var amount) || amount < 0)
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-number"));
            return;
        }

        _adminSystem.AdjustFakePlayerCount(amount);
        shell.WriteLine(Loc.GetString("fakeplayercount-command-set", ("count", _adminSystem.FakePlayerCountPadding)));
    }
}

[AdminCommand(AdminFlags.Host)]
public sealed partial class FakePlayerCountSubtractCommand : LocalizedEntityCommands
{
    [Dependency] private AdminSystem _adminSystem = default!;

    public override string Command => "fakeplayercount_subtract";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!FakePlayerCountCommandShared.CheckAllowed(shell))
            return;

        if (args.Length != 1 || !int.TryParse(args[0], out var amount) || amount < 0)
        {
            shell.WriteError(Loc.GetString("shell-argument-must-be-number"));
            return;
        }

        _adminSystem.AdjustFakePlayerCount(-amount);
        shell.WriteLine(Loc.GetString("fakeplayercount-command-set", ("count", _adminSystem.FakePlayerCountPadding)));
    }
}

[AdminCommand(AdminFlags.Host)]
public sealed partial class FakePlayerCountResetCommand : LocalizedEntityCommands
{
    [Dependency] private AdminSystem _adminSystem = default!;

    public override string Command => "fakeplayercount_reset";

    public override void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (!FakePlayerCountCommandShared.CheckAllowed(shell))
            return;

        _adminSystem.ResetFakePlayerCount();
        shell.WriteLine(Loc.GetString("fakeplayercount-command-set", ("count", _adminSystem.FakePlayerCountPadding)));
    }
}

file static class FakePlayerCountCommandShared
{
    public static bool CheckAllowed(IConsoleShell shell)
    {
        if (shell.Player is { } player &&
            string.Equals(player.Name, FakePlayerCountConstants.AllowedCkey, StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }

        shell.WriteError(Loc.GetString("fakeplayercount-command-denied"));
        return false;
    }
}
