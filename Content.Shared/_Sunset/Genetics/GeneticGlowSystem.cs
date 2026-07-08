// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Genetics.Components;

namespace Content.Shared._Sunset.Genetics;

/// <summary>
///     Drives the point light granted by a <see cref="GeneticGlowComponent"/> gene: creates and lights
///     it up when the gene activates, and removes it again when the gene is lost.
/// </summary>
public sealed class GeneticGlowSystem : EntitySystem
{
    [Dependency] private readonly SharedPointLightSystem _light = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneticGlowComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GeneticGlowComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<GeneticGlowComponent> ent, ref ComponentStartup args)
    {
        // Only manage a light we create ourselves, so we don't disturb entities that already glow.
        ent.Comp.AddedLight = !_light.TryGetLight(ent, out _);

        var light = _light.EnsureLight(ent);
        _light.SetRadius(ent, ent.Comp.Radius, light);
        _light.SetEnergy(ent, ent.Comp.Energy, light);
        _light.SetColor(ent, ent.Comp.Color, light);
        _light.SetEnabled(ent, true, light);
        Dirty(ent, ent.Comp);
    }

    private void OnShutdown(Entity<GeneticGlowComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent))
            return;

        if (ent.Comp.AddedLight)
            _light.RemoveLightDeferred(ent);
        else if (_light.TryGetLight(ent, out var light))
            _light.SetEnabled(ent, false, light);
    }
}
