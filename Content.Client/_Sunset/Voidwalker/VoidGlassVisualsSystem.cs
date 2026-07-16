using System.Collections.Generic;
using Content.Shared._Sunset.Voidwalker;
using Robust.Client.GameObjects;
using Robust.Shared.Maths;

namespace Content.Client._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - tints a wall/window's sprite translucent while VoidGlassComponent is present (see
/// Content.Server._Sunset.Voidwalker.VoidwalkerSystem.OnGlassify), reverting the original color when
/// it's removed.
/// </summary>
public sealed class VoidGlassVisualsSystem : EntitySystem
{
    private static readonly Color GlassTint = new(0.55f, 0.85f, 1f, 0.45f);

    private readonly Dictionary<EntityUid, Color> _originalColors = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidGlassComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<VoidGlassComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<VoidGlassComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SpriteComponent>(ent, out var sprite))
            return;

        _originalColors[ent] = sprite.Color;
        sprite.Color = GlassTint;
    }

    private void OnShutdown(Entity<VoidGlassComponent> ent, ref ComponentShutdown args)
    {
        if (_originalColors.Remove(ent, out var original) && TryComp<SpriteComponent>(ent, out var sprite))
            sprite.Color = original;
    }
}
