using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._Sunset.MartialArts.Components;
using Content.Shared._Sunset.MartialArts.Events;

namespace Content.Shared._Sunset.MartialArts.Systems;

/// <summary>
/// Krav Maga, ported from Goob Station (mini-station-goob). Unlike the combo-based styles, the
/// practitioner primes a technique via an action and the next unarmed strike on a mob applies it:
/// Leg Sweep (knockdown), Neck Chop (mute), Lung Punch (stamina + blocked breathing).
/// </summary>
public sealed partial class SharedMartialArtsSystem
{
    private static readonly TimeSpan KravMagaSweepKnockdown = TimeSpan.FromSeconds(4);
    private static readonly DamageSpecifier KravMagaChopDamage = new() { DamageDict = new() { { "Blunt", 5 } } };

    private static readonly List<string> KravMagaActions = new()
    {
        "ActionKravMagaLegSweep",
        "ActionKravMagaNeckChop",
        "ActionKravMagaLungPunch",
    };

    private void InitializeKravMaga()
    {
        SubscribeLocalEvent<KravMagaComponent, KravMagaActionEvent>(OnKravMagaAction);
        SubscribeLocalEvent<KravMagaComponent, MeleeHitEvent>(OnKravMagaMeleeHit);
    }

    private void GrantKravMaga(EntityUid uid)
    {
        var comp = EnsureComp<KravMagaComponent>(uid);

        // Server-authoritative: action grants from predicted contexts corrupt client state,
        // same reasoning as the other reactive component grants in this system.
        if (_net.IsClient)
            return;

        foreach (var actionId in KravMagaActions)
        {
            if (_actions.AddAction(uid, actionId) is { } action)
                comp.Actions.Add(action);
        }
    }

    private void RevokeKravMaga(EntityUid uid)
    {
        if (!TryComp<KravMagaComponent>(uid, out var comp))
            return;

        foreach (var action in comp.Actions)
            _actions.RemoveAction(action);

        RemComp<KravMagaComponent>(uid);
    }

    private void OnKravMagaAction(Entity<KravMagaComponent> ent, ref KravMagaActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<KravMagaActionComponent>(args.Action.Owner, out var actionComp))
            return;

        args.Handled = true;
        ent.Comp.SelectedMove = actionComp.Move;
        Dirty(ent);

        var moveName = Loc.GetString(actionComp.Move switch
        {
            KravMagaMove.LegSweep => "martial-arts-krav-maga-leg-sweep",
            KravMagaMove.NeckChop => "martial-arts-krav-maga-neck-chop",
            _ => "martial-arts-krav-maga-lung-punch",
        });
        _popup.PopupClient(Loc.GetString("martial-arts-krav-maga-ready", ("move", moveName)), ent, ent);
    }

    private void OnKravMagaMeleeHit(Entity<KravMagaComponent> ent, ref MeleeHitEvent args)
    {
        // Only bare-handed strikes carry the technique.
        if (args.Weapon != ent.Owner || ent.Comp.SelectedMove is not { } move)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(target) || target == ent.Owner)
                continue;

            ApplyKravMagaMove(ent, target, move);

            ent.Comp.SelectedMove = null;
            Dirty(ent);
            break;
        }
    }

    private void ApplyKravMagaMove(Entity<KravMagaComponent> ent, EntityUid target, KravMagaMove move)
    {
        // Look up the priming action's parameters so the yml stays the single source of numbers.
        KravMagaActionComponent? actionComp = null;
        foreach (var action in ent.Comp.Actions)
        {
            if (TryComp<KravMagaActionComponent>(action, out var comp) && comp.Move == move)
            {
                actionComp = comp;
                break;
            }
        }

        var effectTime = TimeSpan.FromSeconds(actionComp?.EffectTime ?? 10f);

        switch (move)
        {
            case KravMagaMove.LegSweep:
                if (_net.IsClient)
                    break;
                _stun.TryKnockdown(target, KravMagaSweepKnockdown, force: true);
                _popup.PopupEntity(Loc.GetString("martial-arts-krav-maga-sweep-hit", ("target", target)), ent, PopupType.MediumCaution);
                break;

            case KravMagaMove.NeckChop:
                if (_net.IsClient)
                    break;
                _damageable.TryChangeDamage(target, KravMagaChopDamage, origin: ent);
                EnsureComp<MutedComponent>(target);
                var mute = EnsureComp<TemporaryMuteComponent>(target);
                mute.ExpiresAt = _timing.CurTime + effectTime;
                _popup.PopupEntity(Loc.GetString("martial-arts-krav-maga-chop-hit", ("target", target)), ent, PopupType.MediumCaution);
                break;

            case KravMagaMove.LungPunch:
                _stamina.TakeStaminaDamage(target, actionComp?.StaminaDamage ?? 40f, source: ent);
                if (_net.IsClient)
                    break;
                var blocked = EnsureComp<BreathingBlockedComponent>(target);
                blocked.ExpiresAt = _timing.CurTime + effectTime;
                _popup.PopupEntity(Loc.GetString("martial-arts-krav-maga-punch-hit", ("target", target)), ent, PopupType.MediumCaution);
                break;
        }
    }
}
