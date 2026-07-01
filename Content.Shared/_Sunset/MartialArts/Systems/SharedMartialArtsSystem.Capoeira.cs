using Content.Shared.Damage;
using Content.Shared.Movement.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._Sunset.Grab.Events;
using Content.Shared._Sunset.MartialArts.Components;
using Robust.Shared.Physics.Components;

namespace Content.Shared._Sunset.MartialArts.Systems;

public sealed partial class SharedMartialArtsSystem
{
    /// <summary>
    /// Hard floor on the attack interval Capoeira's walking-speed bonus can push you to, regardless of
    /// how fast you're moving.
    /// </summary>
    private const float CapoeiraMinAttackInterval = 0.1f;

    private void InitializeCapoeira()
    {
        SubscribeLocalEvent<CapoeiraMomentumComponent, GrabEscalatedEvent>(OnCapoeiraGrabEscalated);
        SubscribeLocalEvent<CapoeiraMomentumComponent, RefreshMovementSpeedModifiersEvent>(OnCapoeiraRefreshSpeed);
        SubscribeLocalEvent<CapoeiraMomentumComponent, GetMeleeAttackRateEvent>(OnCapoeiraGetAttackRate);
    }

    /// <summary>
    /// Continuous walking speeds up unarmed attacks - the faster you're moving, the shorter your
    /// attack cooldown, down to a hard floor of <see cref="CapoeiraMinAttackInterval"/> between hits.
    /// </summary>
    private void OnCapoeiraGetAttackRate(Entity<CapoeiraMomentumComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        if (args.Weapon != ent.Owner || !TryComp<PhysicsComponent>(ent.Owner, out var physics))
            return;

        var speed = physics.LinearVelocity.Length();
        if (speed <= 0f)
            return;

        args.Multipliers *= 1f + speed * 0.15f;

        var effectiveRate = args.Rate * args.Multipliers;
        var maxRate = 1f / CapoeiraMinAttackInterval;
        if (effectiveRate > maxRate)
            args.Multipliers = maxRate / args.Rate;
    }

    private void OnCapoeiraGrabEscalated(Entity<CapoeiraMomentumComponent> ent, ref GrabEscalatedEvent args)
    {
        ent.Comp.AttackSpeedBonusUntil = _timing.CurTime + TimeSpan.FromSeconds(4);
        _movementSpeed.RefreshMovementSpeedModifiers(ent.Owner);
    }

    private void OnCapoeiraRefreshSpeed(Entity<CapoeiraMomentumComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (_timing.CurTime < ent.Comp.AttackSpeedBonusUntil)
            args.ModifySpeed(1.2f);
    }

    private void CapoeiraOnMiss(EntityUid user)
    {
        if (!TryComp<CapoeiraMomentumComponent>(user, out var momentum))
            return;

        momentum.MissBonusUntil = _timing.CurTime + TimeSpan.FromSeconds(3);
    }

    private DamageSpecifier GetCapoeiraDamage(EntityUid user, string type, float amount)
    {
        if (TryComp<CapoeiraMomentumComponent>(user, out var momentum) && _timing.CurTime < momentum.MissBonusUntil)
            amount += (float) momentum.MissDamageBonus;

        return new DamageSpecifier { DamageDict = new() { { type, amount } } };
    }

    /// <summary>
    /// How hard-hitting a velocity-scaled Capoeira move is: faster movement = more power, matching
    /// Goob Station's power formula (velocity * 0.6, clamped between 1x and 4x).
    /// </summary>
    private float GetCapoeiraPower(EntityUid user)
    {
        if (!TryComp<PhysicsComponent>(user, out var physics))
            return 1f;

        return Math.Clamp(physics.LinearVelocity.Length() * 0.6f, 1f, 4f);
    }

    private void CapoeiraPushKick(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, GetCapoeiraDamage(user, "Blunt", 12), origin: user);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(2), force: true);
        _stamina.TakeStaminaDamage(user, -30f);
    }

    private void CapoeiraCircleKick(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, GetCapoeiraDamage(user, "Blunt", 15), origin: user);
        _stamina.TakeStaminaDamage(user, -40f);
    }

    private void CapoeiraSweepKick(EntityUid user, EntityUid target)
    {
        var power = GetCapoeiraPower(user);
        _damageable.TryChangeDamage(target, GetCapoeiraDamage(user, "Blunt", 10 * power), origin: user);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(1 * power), force: true);
        _stamina.TakeStaminaDamage(target, 10f, source: user);
        _stamina.TakeStaminaDamage(user, -60f);
        GrantAttackSpeedBuff(user);
    }

    private void CapoeiraSpinKick(EntityUid user, EntityUid target)
    {
        var power = GetCapoeiraPower(user);
        _damageable.TryChangeDamage(target, GetCapoeiraDamage(user, "Blunt", 25 * power), origin: user);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(2 * power), force: true);
        _stamina.TakeStaminaDamage(target, 15f, source: user);
        _stamina.TakeStaminaDamage(user, -100f);
        GrantAttackSpeedBuff(user);
    }

    private void GrantAttackSpeedBuff(EntityUid user)
    {
        if (!TryComp<CapoeiraMomentumComponent>(user, out var momentum))
            return;

        momentum.AttackSpeedBonusUntil = _timing.CurTime + TimeSpan.FromSeconds(5);
        _movementSpeed.RefreshMovementSpeedModifiers(user);
    }

    /// <summary>
    /// Self-targeted recovery move: two disarm-intent clicks on yourself while knocked down instantly
    /// gets you back on your feet, matching Goob Station's "Kick Up".
    /// </summary>
    private void CapoeiraKickUp(EntityUid user)
    {
        RemComp<KnockedDownComponent>(user);
    }
}
