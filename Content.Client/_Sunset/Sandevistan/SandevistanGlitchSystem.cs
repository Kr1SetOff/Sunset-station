using Content.Shared._Sunset.Sandevistan;
using Robust.Client.Graphics;
using Robust.Client.Player;
using Robust.Shared.Player;

namespace Content.Client._Sunset.Sandevistan;

/// <summary>
/// Adds/removes the screen-glitch overlay for the local player based on SandevistanGlitchComponent
/// (added by SandevistanDisableEffect when the overload meter maxes out).
/// </summary>
public sealed class SandevistanGlitchSystem : EntitySystem
{
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IOverlayManager _overlayMan = default!;

    private SandevistanGlitchOverlay _overlay = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanGlitchComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanGlitchComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<SandevistanGlitchComponent, LocalPlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<SandevistanGlitchComponent, LocalPlayerDetachedEvent>(OnPlayerDetached);

        _overlay = new();
    }

    private void OnPlayerAttached(Entity<SandevistanGlitchComponent> ent, ref LocalPlayerAttachedEvent args) =>
        _overlayMan.AddOverlay(_overlay);

    private void OnPlayerDetached(Entity<SandevistanGlitchComponent> ent, ref LocalPlayerDetachedEvent args) =>
        _overlayMan.RemoveOverlay(_overlay);

    private void OnInit(Entity<SandevistanGlitchComponent> ent, ref ComponentInit args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;

        _overlayMan.AddOverlay(_overlay);
    }

    private void OnShutdown(Entity<SandevistanGlitchComponent> ent, ref ComponentShutdown args)
    {
        if (_player.LocalEntity != ent.Owner)
            return;

        _overlayMan.RemoveOverlay(_overlay);
    }
}
