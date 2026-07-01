using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared._Sunset.Grab;
using Content.Shared._Sunset.Grab.Components;

namespace Content.Shared._Sunset.MartialArts.Systems;

public sealed partial class SharedMartialArtsSystem
{
    private static readonly DamageSpecifier CqcBluntDamage = new() { DamageDict = new() { { "Blunt", 10 } } };

    private void CqcSlam(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, CqcBluntDamage, origin: user);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(12), force: true);
    }

    private void CqcKick(EntityUid user, EntityUid target)
    {
        // Kicking someone who's already down hits much harder.
        var downed = HasComp<Content.Shared.Stunnable.KnockedDownComponent>(target);
        var damage = downed ? 25f : 10f;
        var stamina = downed ? 55f : 25f;

        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", damage } } }, origin: user);
        _stamina.TakeStaminaDamage(target, stamina, source: user);
    }

    private void CqcRestrain(EntityUid user, EntityUid target)
    {
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(10), force: true);
        _stamina.TakeStaminaDamage(target, 30f, source: user);
    }

    private void CqcPressure(EntityUid user, EntityUid target)
    {
        _stamina.TakeStaminaDamage(target, 65f, source: user);
    }

    private void CqcConsecutive(EntityUid user, EntityUid target)
    {
        // Snap-neck execution: only if we're actively choking a target who is stamina-critical.
        if (TryComp<GrabbableComponent>(target, out var grabbable) &&
            grabbable.Grabber == user &&
            grabbable.Stage == GrabStage.Choke &&
            TryComp<StaminaComponent>(target, out var stamina) &&
            stamina.Critical &&
            HasComp<MobStateComponent>(target))
        {
            _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 200 } } }, ignoreResistances: true, origin: user);
            _popup.PopupEntity(Loc.GetString("martial-arts-cqc-snap-neck", ("target", target)), user, PopupType.LargeCaution);
            return;
        }

        _damageable.TryChangeDamage(target, CqcBluntDamage, origin: user);
    }
}
