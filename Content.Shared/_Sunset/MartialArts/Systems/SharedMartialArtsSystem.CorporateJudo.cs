using Content.Shared.Clothing;
using Content.Shared.Damage;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared._Sunset.MartialArts.Components;

namespace Content.Shared._Sunset.MartialArts.Systems;

public sealed partial class SharedMartialArtsSystem
{
    private void InitializeCorporateJudo()
    {
        SubscribeLocalEvent<GrantMartialArtOnEquipComponent, ClothingGotEquippedEvent>(OnGrantMartialArtOnEquip);
        SubscribeLocalEvent<GrantMartialArtOnEquipComponent, ClothingGotUnequippedEvent>(OnRevokeMartialArtOnUnequip);

        SubscribeLocalEvent<ArmbarredComponent, StoodEvent>(OnArmbarredStood);
        SubscribeLocalEvent<ArmbarredComponent, PullStoppedMessage>(OnArmbarStopped);
    }

    private void OnGrantMartialArtOnEquip(Entity<GrantMartialArtOnEquipComponent> ent, ref ClothingGotEquippedEvent args)
    {
        TryGrantMartialArt(args.Wearer, ent.Comp.Style);
    }

    private void OnRevokeMartialArtOnUnequip(Entity<GrantMartialArtOnEquipComponent> ent, ref ClothingGotUnequippedEvent args)
    {
        if (!TryComp<MartialArtsKnowledgeComponent>(args.Wearer, out var knowledge) || knowledge.Style != ent.Comp.Style)
            return;

        RemComp<MartialArtsKnowledgeComponent>(args.Wearer);
    }

    /// <summary>
    /// Slows the target and knocks the wind out of them - the opener that doesn't require a downed target.
    /// </summary>
    private void JudoDiscombobulate(EntityUid user, EntityUid target)
    {
        _stamina.TakeStaminaDamage(target, 20f, source: user);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(1), force: true);
    }

    private void JudoEyePoke(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 5 } } }, origin: user);
        _stamina.TakeStaminaDamage(target, 15f, source: user);
    }

    /// <summary>
    /// A standard hip throw - only works on a target that's still on their feet.
    /// </summary>
    private void JudoThrow(EntityUid user, EntityUid target)
    {
        if (HasComp<KnockedDownComponent>(target))
            return;

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, user);

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(4), force: true);
        _stamina.TakeStaminaDamage(target, 5f, source: user);
    }

    /// <summary>
    /// Only works on a downed target - locks them in an armbar hold, forcing them to stay down until the
    /// hold breaks. Sets up Wheel Throw, which only the same attacker can follow up with.
    /// </summary>
    private void JudoArmbar(EntityUid user, EntityUid target)
    {
        if (!HasComp<KnockedDownComponent>(target))
            return;

        if (!HasComp<ArmbarredComponent>(target))
        {
            _stamina.TakeStaminaDamage(target, 10f, source: user);
            EnsureComp<ArmbarredComponent>(target).Puller = user;
        }

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(5), force: true);
    }

    /// <summary>
    /// The finisher - only works on a target you've personally already got in an armbar. Throws them away
    /// and gets you back on your feet.
    /// </summary>
    private void JudoWheelThrow(EntityUid user, EntityUid target)
    {
        if (!TryComp<ArmbarredComponent>(target, out var armbarred) || armbarred.Puller != user)
            return;

        _stamina.TakeStaminaDamage(target, 120f, source: user);

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, user);

        ThrowAt(user, target, 5f);

        _stun.TryKnockdown(user, TimeSpan.Zero, force: true);
        RemComp<KnockedDownComponent>(user);
    }

    private void OnArmbarredStood(Entity<ArmbarredComponent> ent, ref StoodEvent args)
    {
        if (TryComp<PullableComponent>(ent.Owner, out var pullable))
            _pulling.TryStopPull(ent.Owner, pullable, ent.Comp.Puller);

        RemComp<ArmbarredComponent>(ent.Owner);
    }

    private void OnArmbarStopped(Entity<ArmbarredComponent> ent, ref PullStoppedMessage args)
    {
        if (args.PullerUid != ent.Comp.Puller)
            return;

        RemComp<KnockedDownComponent>(ent.Owner);
        RemComp<ArmbarredComponent>(ent.Owner);
    }
}
