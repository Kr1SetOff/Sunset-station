// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Goobstation.Weapons.KnockdownOnHit;

public sealed class KnockdownOnHitSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KnockdownOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<KnockdownOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        if (ent.Comp.KnockdownOnHeavyAttack && args.Direction == null)
            return;

        var duration = TimeSpan.FromSeconds(ent.Comp.Duration);
        foreach (var target in args.HitEntities)
        {
            _stun.TryKnockdown(target, duration, autoStand: ent.Comp.Autostand, drop: ent.Comp.DropItems);
        }
    }
}
