// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared._Sunset.Genetics;

/// <summary>
///     Applies the speed modifier from a <see cref="GeneticSpeedComponent"/> gene to its carrier,
///     refreshing movement speed when the gene is gained or lost. Mirrors
///     <see cref="Content.Shared._DV.Carrying.CarryingSlowdownSystem"/>.
/// </summary>
public sealed class GeneticSpeedSystem : EntitySystem
{
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneticSpeedComponent, ComponentStartup>(OnChanged);
        SubscribeLocalEvent<GeneticSpeedComponent, ComponentShutdown>(OnChanged);
        SubscribeLocalEvent<GeneticSpeedComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMoveSpeed);
    }

    private void OnChanged<T>(EntityUid uid, GeneticSpeedComponent component, T args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(uid);
    }

    private void OnRefreshMoveSpeed(Entity<GeneticSpeedComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.WalkModifier, ent.Comp.SprintModifier);
    }
}
