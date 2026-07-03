// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Temperature.Components;
using Content.Shared._Sunset.Genetics.Components;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     Implements the thermal-regulation gene: while <see cref="GeneThermalComponent"/> is present, the
///     carrier's temperature damage thresholds are pushed far apart so heat and cold can't hurt them.
///     The original thresholds are restored when the gene is lost. We do this ourselves because the
///     fork's <c>TemperatureImmunityComponent</c> is not read by the temperature system.
/// </summary>
public sealed class GeneThermalSystem : EntitySystem
{
    private const float ImmuneHeatThreshold = 100000f;
    private const float ImmuneColdThreshold = 0f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneThermalComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GeneThermalComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<GeneThermalComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<TemperatureDamageComponent>(ent, out var temp))
            return;

        ent.Comp.OldHeatThreshold = temp.HeatDamageThreshold;
        ent.Comp.OldColdThreshold = temp.ColdDamageThreshold;
        ent.Comp.Stored = true;

        // Cryogenesis (ColdOnly) only widens the cold threshold; full thermal regulation widens both.
        if (!ent.Comp.ColdOnly)
            temp.HeatDamageThreshold = ImmuneHeatThreshold;
        temp.ColdDamageThreshold = ImmuneColdThreshold;
    }

    private void OnShutdown(Entity<GeneThermalComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent) || !ent.Comp.Stored || !TryComp<TemperatureDamageComponent>(ent, out var temp))
            return;

        if (!ent.Comp.ColdOnly)
            temp.HeatDamageThreshold = ent.Comp.OldHeatThreshold;
        temp.ColdDamageThreshold = ent.Comp.OldColdThreshold;
    }
}
