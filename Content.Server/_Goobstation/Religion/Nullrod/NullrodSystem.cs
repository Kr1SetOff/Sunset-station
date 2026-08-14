using Content.Server.Bible.Components;
using Content.Shared._Starlight.Vampire.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Verbs;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Religion.Nullrod;

/// <summary>
/// Ported from Goob-Station's Religion system (SharedNullRodSystem + BindNullrodSystem), adapted
/// to this fork's server-only Bible architecture: AttackAttemptEvent is only ever raised directed
/// at the attacker (not broadcast, and not raised on the weapon), so unlike Goob's version this
/// subscribes on HandsComponent (anyone capable of wielding a melee weapon) and inspects
/// args.Weapon directly instead of subscribing on NullrodComponent itself.
/// </summary>
public sealed class NullrodSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HandsComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<NullrodComponent, EntGotInsertedIntoContainerMessage>(OnInsertedContainer);
        SubscribeLocalEvent<NullrodComponent, GetVerbsEvent<ActivationVerb>>(OnGetBindVerb);
    }

    private void OnAttackAttempt(Entity<HandsComponent> ent, ref AttackAttemptEvent args)
    {
        if (args.Weapon is not { } weapon || !TryComp<NullrodComponent>(weapon.Owner, out var nullrod))
            return;

        if (!nullrod.UntrainedUseRestriction || HasComp<BibleUserComponent>(args.Uid))
            return;

        args.Cancel();
        PunishUntrained(weapon.Owner, nullrod, args.Uid);
    }

    private void PunishUntrained(EntityUid nullrodUid, NullrodComponent nullrod, EntityUid user)
    {
        if (_timing.CurTime < nullrod.NextPopupTime)
            return;

        if (!_damageable.TryChangeDamage(user, nullrod.DamageOnUntrainedUse, origin: nullrodUid))
            return;

        _popup.PopupEntity(Loc.GetString(nullrod.UntrainedUseString), user, user, PopupType.MediumCaution);
        _audio.PlayPvs(nullrod.UntrainedUseSound, user);

        nullrod.NextPopupTime = _timing.CurTime + nullrod.PopupCooldown;
    }

    // Mirrors BibleSystem.OnInsertedContainer: an unholy creature that picks up a nullrod gets
    // burned and knocked down for its trouble.
    private void OnInsertedContainer(EntityUid uid, NullrodComponent component, EntGotInsertedIntoContainerMessage args)
    {
        if (!HasComp<UnholyComponent>(args.Container.Owner))
            return;

        Timer.Spawn(500, () =>
        {
            if (TerminatingOrDeleted(args.Container.Owner))
                return;

            _stun.TryUpdateParalyzeDuration(args.Container.Owner, TimeSpan.FromSeconds(10));
            _damageable.TryChangeDamage(args.Container.Owner, component.DamageOnUntrainedUse, origin: uid);
            _audio.PlayPvs(component.UntrainedUseSound, args.Container.Owner);
        });
    }

    private void OnGetBindVerb(Entity<NullrodComponent> ent, ref GetVerbsEvent<ActivationVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !ent.Comp.Recallable)
            return;

        if (!TryComp<BibleUserComponent>(args.User, out var bibleUser) || bibleUser.NullRod == ent.Owner)
            return;

        var user = args.User;
        var nullrod = ent.Owner;
        args.Verbs.Add(new ActivationVerb
        {
            Text = Loc.GetString("nullrod-bind-verb"),
            Act = () =>
            {
                bibleUser.NullRod = nullrod;
                _popup.PopupEntity(Loc.GetString("nullrod-bind-verb-done", ("nullrod", nullrod)), user, user);
            },
        });
    }
}
