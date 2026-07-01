using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._Sunset.MartialArts.Components;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.MartialArts.Systems;

public sealed partial class SharedMartialArtsSystem
{
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;

    private static readonly ProtoId<AlertCategoryPrototype> NinjutsuSneakAlertCategory = "NinjutsuSneak";
    private static readonly ProtoId<AlertPrototype> NinjutsuSneakReadyAlert = "NinjutsuSneakReady";
    private static readonly ProtoId<AlertPrototype> NinjutsuSneakLostAlert = "NinjutsuSneakLost";

    private void InitializeNinjutsu()
    {
        SubscribeLocalEvent<NinjutsuSneakAttackComponent, MeleeHitEvent>(OnNinjutsuMeleeHit);
        SubscribeLocalEvent<NinjutsuSneakAttackComponent, UserInteractHandEvent>(OnNinjutsuHug);
        SubscribeLocalEvent<NinjutsuSneakAttackComponent, RefreshMovementSpeedModifiersEvent>(OnNinjutsuRefreshSpeed);
        SubscribeLocalEvent<NinjutsuSneakAttackComponent, ComponentInit>(OnNinjutsuSneakInit);
        SubscribeLocalEvent<NinjutsuSneakAttackComponent, ComponentShutdown>(OnNinjutsuSneakShutdown);
    }

    private void OnNinjutsuSneakInit(Entity<NinjutsuSneakAttackComponent> ent, ref ComponentInit args)
    {
        UpdateNinjutsuSneakAlert(ent);
    }

    private void OnNinjutsuSneakShutdown(Entity<NinjutsuSneakAttackComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent.Owner))
            return;

        _alerts.ClearAlertCategory(ent.Owner, NinjutsuSneakAlertCategory);
    }

    private void UpdateNinjutsuSneakAlert(Entity<NinjutsuSneakAttackComponent> ent)
    {
        _alerts.ShowAlert(ent.Owner, ent.Comp.Revealed ? NinjutsuSneakLostAlert : NinjutsuSneakReadyAlert);
    }

    private void OnNinjutsuMeleeHit(Entity<NinjutsuSneakAttackComponent> ent, ref MeleeHitEvent args)
    {
        if (args.Weapon != ent.Owner || args.HitEntities.Count == 0)
            return;

        var target = args.HitEntities[0];

        if (!ent.Comp.Revealed)
        {
            // Assassinate: a huge bonus on the first hit while still hidden.
            args.BonusDamage += new DamageSpecifier { DamageDict = new() { { "Blunt", ent.Comp.AssassinateBonusDamage } } };
            RevealSneakAttack(ent);
        }
        else if (HasComp<KnockedDownComponent>(target))
        {
            // Swift Strike: finishing off a downed target you're no longer hidden from costs them
            // extra stamina, but slows your own follow-up attacks down.
            _stamina.TakeStaminaDamage(target, ent.Comp.SwiftStrikeStaminaDamage, source: ent.Owner);

            if (TryComp<MeleeWeaponComponent>(ent.Owner, out var meleeWeapon))
            {
                meleeWeapon.NextAttack += ent.Comp.SwiftStrikeExtraCooldown;
                DirtyField(ent.Owner, meleeWeapon, nameof(MeleeWeaponComponent.NextAttack));
            }
        }

        foreach (var hitEntity in args.HitEntities)
            CheckNinjutsuKillBonus(ent.Owner, hitEntity);
    }

    private void OnNinjutsuRefreshSpeed(Entity<NinjutsuSneakAttackComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (_timing.CurTime < ent.Comp.KillBonusUntil)
            args.ModifySpeed(ent.Comp.KillMoveSpeedBonus);
    }

    private void OnNinjutsuHug(Entity<NinjutsuSneakAttackComponent> ent, ref UserInteractHandEvent args)
    {
        if (args.Handled || !HasComp<Content.Shared.Mobs.Components.MobStateComponent>(args.Target))
            return;

        if (HasComp<Content.Shared.Stunnable.KnockedDownComponent>(args.Target))
            return; // only works on a standing target

        var backstab = !ent.Comp.Revealed;
        var multiplier = backstab ? ent.Comp.BackstabDamageMultiplier : 1f;

        var slowdown = ent.Comp.TakedownSlowdownTime * multiplier;
        var muteTime = ent.Comp.TakedownMuteTime * multiplier;

        _stun.TryKnockdown(args.Target, slowdown, force: true);

        EnsureComp<MutedComponent>(args.Target);
        var tempMute = EnsureComp<TemporaryMuteComponent>(args.Target);
        tempMute.ExpiresAt = _timing.CurTime + muteTime;

        if (backstab)
            RevealSneakAttack(ent);

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("martial-arts-ninjutsu-takedown", ("target", args.Target)), ent.Owner, PopupType.Medium);
    }

    private void RevealSneakAttack(Entity<NinjutsuSneakAttackComponent> ent)
    {
        ent.Comp.Revealed = true;
        ent.Comp.RevealedUntil = _timing.CurTime + ent.Comp.RevealDuration;
        Dirty(ent);
        UpdateNinjutsuSneakAlert(ent);
    }

    private void CheckNinjutsuKillBonus(EntityUid user, EntityUid target)
    {
        if (!TryComp<NinjutsuSneakAttackComponent>(user, out var sneak))
            return;

        if (!_mobState.IsDead(target))
            return;

        sneak.KillBonusUntil = _timing.CurTime + sneak.KillMoveSpeedBonusDuration;
        _movementSpeed.RefreshMovementSpeedModifiers(user);
        _popup.PopupEntity(Loc.GetString("martial-arts-ninjutsu-kill-bonus"), user, user, PopupType.Medium);
    }

    private void BiteTheDust(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 10 } } }, origin: user);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(9), force: true);
        CheckNinjutsuKillBonus(user, target);
    }

    private void DirtyKill(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 20 } } }, origin: user);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(7), force: true);
        CheckNinjutsuKillBonus(user, target);
    }
}
