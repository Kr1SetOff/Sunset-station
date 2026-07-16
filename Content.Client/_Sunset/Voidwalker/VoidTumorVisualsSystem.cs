using System;
using System.Collections.Generic;
using Content.Shared._Sunset.Voidwalker;
using Robust.Client.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Timing;

namespace Content.Client._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - progressively tints a Kidnap victim darker the longer their void tumor (see
/// VoidTumorComponent) has been left in, reverting them if it's surgically removed in time, or
/// leaving the tint permanent if VoidConsumedComponent gets added once it finishes.
/// </summary>
public sealed class VoidTumorVisualsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private static readonly Color VoidTint = new(0.35f, 0.2f, 0.45f);

    private readonly Dictionary<EntityUid, Color> _originalColors = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidTumorComponent, ComponentShutdown>(OnTumorShutdown);
        SubscribeLocalEvent<VoidConsumedComponent, ComponentStartup>(OnConsumedStartup);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<VoidTumorComponent>();
        while (query.MoveNext(out _, out var tumor))
        {
            if (!TryComp<SpriteComponent>(tumor.Victim, out var sprite))
                continue;

            if (!_originalColors.ContainsKey(tumor.Victim))
                _originalColors[tumor.Victim] = sprite.Color;

            var progress = tumor.EndTime > tumor.StartTime
                ? Math.Clamp((float) ((now - tumor.StartTime) / (tumor.EndTime - tumor.StartTime)), 0f, 1f)
                : 0f;

            sprite.Color = Color.InterpolateBetween(_originalColors[tumor.Victim], VoidTint, progress);
        }
    }

    private void OnTumorShutdown(Entity<VoidTumorComponent> ent, ref ComponentShutdown args)
    {
        // Cured before completion - restore the victim's original color, unless they've already
        // fully transformed (VoidConsumedComponent owns the tint from that point on).
        if (HasComp<VoidConsumedComponent>(ent.Comp.Victim))
            return;

        if (_originalColors.Remove(ent.Comp.Victim, out var original) && TryComp<SpriteComponent>(ent.Comp.Victim, out var sprite))
            sprite.Color = original;
    }

    private void OnConsumedStartup(Entity<VoidConsumedComponent> ent, ref ComponentStartup args)
    {
        if (TryComp<SpriteComponent>(ent, out var sprite))
            sprite.Color = VoidTint;
    }
}
