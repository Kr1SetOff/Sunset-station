// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Sprite;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;

namespace Content.Server._Sunset.Genetics;

public sealed class GeneticSizeSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GeneticSizeComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GeneticSizeComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<GeneticSizeComponent> ent, ref ComponentStartup args)
    {
        Rescale(ent, ent.Comp.Scale);
    }

    private void OnShutdown(Entity<GeneticSizeComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent) || ent.Comp.Scale == 0f)
            return;
        Rescale(ent, 1f / ent.Comp.Scale);
    }

    private void Rescale(EntityUid uid, float factor)
    {
        EnsureComp<ScaleVisualsComponent>(uid);
        var appearance = EnsureComp<AppearanceComponent>(uid);

        if (!_appearance.TryGetData<Vector2>(uid, ScaleVisuals.Scale, out var oldScale, appearance))
            oldScale = Vector2.One;

        _appearance.SetData(uid, ScaleVisuals.Scale, oldScale * factor, appearance);
    }
}
