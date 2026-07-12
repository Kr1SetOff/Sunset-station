// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Eye.Blinding.Systems;

namespace Content.Shared._Sunset.Genetics;

/// <summary>
///     Drives <see cref="GeneNearsightedComponent"/>: applies a partial eye-damage floor on activation and
///     clears it again on removal, blurring the carrier's vision without fully blinding them.
/// </summary>
public sealed class GeneNearsightedSystem : EntitySystem
{
    [Dependency] private readonly BlindableSystem _blinding = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneNearsightedComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GeneNearsightedComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<GeneNearsightedComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<BlindableComponent>(ent, out var blindable))
            return;

        _blinding.SetMinDamage((ent.Owner, blindable), ent.Comp.Blindness);
    }

    private void OnShutdown(Entity<GeneNearsightedComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent) || !TryComp<BlindableComponent>(ent, out var blindable))
            return;

        if (blindable.MinDamage != 0)
            _blinding.SetMinDamage((ent.Owner, blindable), 0);
    }
}
