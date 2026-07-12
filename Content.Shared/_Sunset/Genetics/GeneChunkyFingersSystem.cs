// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Interaction.Components;

namespace Content.Shared._Sunset.Genetics;

/// <summary>
///     Enforces <see cref="GeneChunkyFingersComponent"/>: removes the carrier's
///     <see cref="ComplexInteractionComponent"/> while the gene is active, blocking fine manipulation
///     (wiring, taking pills, disarming...), and restores it again once the gene deactivates.
/// </summary>
public sealed class GeneChunkyFingersSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneChunkyFingersComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GeneChunkyFingersComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<GeneChunkyFingersComponent> ent, ref ComponentStartup args)
    {
        if (!HasComp<ComplexInteractionComponent>(ent))
            return;

        RemComp<ComplexInteractionComponent>(ent);
        ent.Comp.RemovedComplexInteraction = true;
    }

    private void OnShutdown(Entity<GeneChunkyFingersComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent) || !ent.Comp.RemovedComplexInteraction)
            return;

        EnsureComp<ComplexInteractionComponent>(ent);
    }
}
