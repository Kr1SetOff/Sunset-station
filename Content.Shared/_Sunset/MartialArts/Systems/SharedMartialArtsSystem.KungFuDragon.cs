using Content.Shared.Damage;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._Sunset.MartialArts.Components;
using Robust.Shared.Physics.Components;

namespace Content.Shared._Sunset.MartialArts.Systems;

public sealed partial class SharedMartialArtsSystem
{
    private void InitializeKungFuDragon()
    {
        SubscribeLocalEvent<DragonPowerComponent, GetMeleeDamageEvent>(OnDragonGetDamage);
    }

    private void OnDragonGetDamage(Entity<DragonPowerComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (args.Weapon != ent.Owner || _timing.CurTime >= ent.Comp.PowerBuffUntil)
            return;

        args.Modifiers.Add(new DamageModifierSet
        {
            Coefficients = new()
            {
                { "Blunt", ent.Comp.DamageMultiplier },
            },
        });
    }

    /// <summary>
    /// Standing still (below the component's velocity threshold) for a moment builds up power for a
    /// temporary damage buff. Runs every tick for everyone who knows Kung Fu Dragon.
    /// </summary>
    private void UpdateKungFuDragon(TimeSpan curTime)
    {
        var query = EntityQueryEnumerator<DragonPowerComponent, PhysicsComponent>();
        while (query.MoveNext(out var uid, out var dragon, out var physics))
        {
            if (physics.LinearVelocity.LengthSquared() > dragon.MinVelocitySquared)
            {
                dragon.LastMoveTime = curTime;
                continue;
            }

            if (curTime < dragon.LastMoveTime + dragon.PauseDuration)
                continue;

            dragon.PowerBuffUntil = curTime + dragon.BuffLength;
            // So this doesn't keep re-popping every tick once the pause threshold is crossed.
            dragon.LastMoveTime = curTime;
        }
    }

    private void DragonClaw(EntityUid user, EntityUid target)
    {
        _stamina.TakeStaminaDamage(target, 50f, source: user);
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 10 } } }, origin: user);
    }

    private void DragonTail(EntityUid user, EntityUid target)
    {
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, user);

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(6), force: true);
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 15 } } }, origin: user);
    }

    private void DragonStrike(EntityUid user, EntityUid target)
    {
        if (!HasComp<KnockedDownComponent>(target))
        {
            _popup.PopupClient(Loc.GetString("martial-arts-fail-target-standing"), user, user);
            return;
        }

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(2), force: true);
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 40 } } }, origin: user);
    }
}
