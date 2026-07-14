using Content.Client._Sunset.Arena;
using Content.Client.Gameplay;
using Content.Client.Ghost;
using Content.Client.Lobby; //🌟Starlight🌟
using Content.Client.UserInterface.Systems.Gameplay;
using Content.Client.UserInterface.Systems.Ghost.Widgets;
using Content.Shared._Sunset.Arena;
using Content.Shared.Ghost;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controllers;

namespace Content.Client.UserInterface.Systems.Ghost;

// TODO hud refactor BEFORE MERGE fix ghost gui being too far up
public sealed partial class GhostUIController : UIController, IOnSystemChanged<GhostSystem>, IOnSystemChanged<ArenaSystem>
{
    [Dependency] private IEntityNetworkManager _net = default!;

    [UISystemDependency] private readonly GhostSystem? _system = default;
    [UISystemDependency] private readonly ArenaSystem? _arenaSystem = default; // 🌇Sunset🌇

    private ArenaWindow? _arenaWindow; // 🌇Sunset🌇

    private GhostGui? Gui => UIManager.GetActiveUIWidgetOrNull<GhostGui>();

    public override void Initialize()
    {
        base.Initialize();

        var gameplayStateLoad = UIManager.GetUIController<GameplayStateLoadController>();
        gameplayStateLoad.OnScreenLoad += OnScreenLoad;
        gameplayStateLoad.OnScreenUnload += OnScreenUnload;
    }

    private void OnScreenLoad()
    {
        LoadGui();
    }

    private void OnScreenUnload()
    {
        UnloadGui();
    }

    public void OnSystemLoaded(ArenaSystem system) // 🌇Sunset🌇
    {
        system.StatusChanged += OnArenaStatusChanged;
    }

    public void OnSystemUnloaded(ArenaSystem system) // 🌇Sunset🌇
    {
        system.StatusChanged -= OnArenaStatusChanged;
    }

    public void OnSystemLoaded(GhostSystem system)
    {
        system.PlayerRemoved += OnPlayerRemoved;
        system.PlayerUpdated += OnPlayerUpdated;
        system.PlayerAttached += OnPlayerAttached;
        system.PlayerDetached += OnPlayerDetached;
        system.GhostWarpsResponse += OnWarpsResponse;
        system.GhostRoleCountUpdated += OnRoleCountUpdated;
    }

    public void OnSystemUnloaded(GhostSystem system)
    {
        system.PlayerRemoved -= OnPlayerRemoved;
        system.PlayerUpdated -= OnPlayerUpdated;
        system.PlayerAttached -= OnPlayerAttached;
        system.PlayerDetached -= OnPlayerDetached;
        system.GhostWarpsResponse -= OnWarpsResponse;
        system.GhostRoleCountUpdated -= OnRoleCountUpdated;
    }

    public void UpdateGui()
    {
        if (Gui == null)
        {
            return;
        }

        Gui.Visible = _system?.IsGhost ?? false;
        Gui.Update(_system?.AvailableGhostRoleCount, _system?.Player?.CanReturnToBody);
    }

    private void OnPlayerRemoved(GhostComponent component)
    {
        Gui?.Hide();
    }

    private void OnPlayerUpdated(GhostComponent component)
    {
        UpdateGui();
    }

    private void OnPlayerAttached(GhostComponent component)
    {
        if (Gui == null)
            return;

        Gui.Visible = true;
        UpdateGui();
    }

    private void OnPlayerDetached()
    {
        Gui?.Hide();
    }

    private void OnWarpsResponse(GhostWarpsResponseEvent msg)
    {
        if (Gui?.TargetWindow is not { } window)
            return;

        window.UpdateWarps(msg.Warps);
        window.Populate();
    }

    private void OnRoleCountUpdated(GhostUpdateGhostRoleCountEvent msg)
    {
        UpdateGui();
    }

    private void OnWarpClicked(NetEntity player)
    {
        var msg = new GhostWarpToTargetRequestEvent(player);
        _net.SendSystemNetworkMessage(msg);
    }

    private void OnGhostnadoClicked()
    {
        var msg = new GhostnadoRequestEvent();
        _net.SendSystemNetworkMessage(msg);
    }

    public void LoadGui()
    {
        if (Gui == null)
            return;

        Gui.RequestWarpsPressed += RequestWarps;
        Gui.ReturnToBodyPressed += ReturnToBody;
        Gui.GhostRolesPressed += GhostRolesPressed;
        Gui.NewLifePressed += NewLifePressed; //🌟Starlight🌟
        Gui.CharacterEditorPressed += CharacterEditorPressed; //🌟Starlight🌟
        Gui.GhostThemePressed += GhostThemePressed; //🌟Starlight🌟
        Gui.ArenaPressed += ArenaPressed; // 🌇Sunset🌇
        Gui.TargetWindow.WarpClicked += OnWarpClicked;
        Gui.TargetWindow.OnGhostnadoClicked += OnGhostnadoClicked;

        UpdateGui();
        Gui.UpdateArena(GetArenaQueueCount()); // 🌇Sunset🌇
    }

    public void UnloadGui()
    {
        if (Gui == null)
            return;

        Gui.RequestWarpsPressed -= RequestWarps;
        Gui.ReturnToBodyPressed -= ReturnToBody;
        Gui.GhostRolesPressed -= GhostRolesPressed;
        Gui.NewLifePressed -= NewLifePressed; //🌟Starlight🌟
        Gui.CharacterEditorPressed -= CharacterEditorPressed; //🌟Starlight🌟
        Gui.GhostThemePressed -= GhostThemePressed; //🌟Starlight🌟
        Gui.ArenaPressed -= ArenaPressed; // 🌇Sunset🌇
        Gui.TargetWindow.WarpClicked -= OnWarpClicked;

        Gui.Hide();
    }

    private void ReturnToBody()
    {
        _system?.ReturnToBody();
    }

    private void RequestWarps()
    {
        _system?.RequestWarps();
        Gui?.TargetWindow.Populate();
        Gui?.TargetWindow.OpenCentered();
    }

    private void GhostRolesPressed()
    {
        _system?.OpenGhostRoles();
    }

    private void NewLifePressed() //🌟Starlight🌟
        =>  _system?.OpenNewLife();

    private void CharacterEditorPressed() //🌟Starlight🌟
        => UIManager.GetUIController<LobbyUIController>().OpenCharacterSetupWindow();

    private void GhostThemePressed() //🌟Starlight🌟
        => _system?.OpenGhostTheme();

    // 🌇Sunset🌇 - the arena window is a plain client control driven by network events (not tied to
    // an entity/EUI), so it can just be opened/closed directly instead of round-tripping a console
    // command like the other ghost windows above.
    private void ArenaPressed()
    {
        _arenaWindow ??= new ArenaWindow();

        if (_arenaWindow.IsOpen)
            _arenaWindow.Close();
        else
            _arenaWindow.OpenCentered();
    }

    private void OnArenaStatusChanged(ArenaStatusEvent status)
    {
        Gui?.UpdateArena(GetArenaQueueCount());
    }

    private int? GetArenaQueueCount()
    {
        var status = _arenaSystem?.LastStatus;
        return status is { State: ArenaState.Queueing } ? status.Participants : null;
    }
}
