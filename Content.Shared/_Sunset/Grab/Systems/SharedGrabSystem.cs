using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared._Sunset.Grab.Components;
using Content.Shared._Sunset.Grab.Events;
using Robust.Shared.Physics.Events;
using Robust.Shared.Timing;

namespace Content.Shared._Sunset.Grab.Systems;

/// <summary>
/// Drives the three-stage grab escalation (Passive -&gt; Aggressive -&gt; Choke) that sits on top of the
/// vanilla pulling system. Passive is just a normal pull; escalating further is done by intercepting
/// combat-mode melee clicks against the entity being pulled instead of dealing weapon damage.
/// </summary>
public sealed class SharedGrabSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PullerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<PullerComponent, PullStoppedMessage>(OnPullStopped);

        SubscribeLocalEvent<GrabberComponent, AttemptMeleeEvent>(OnAttemptMelee);
        SubscribeLocalEvent<GrabberComponent, GrabThrowActionEvent>(OnThrowAction);
        SubscribeLocalEvent<GrabberComponent, ComponentShutdown>(OnGrabberShutdown);

        SubscribeLocalEvent<GrabbableComponent, ComponentShutdown>(OnGrabbableShutdown);

        SubscribeLocalEvent<GrabThrownComponent, StartCollideEvent>(OnGrabThrownCollide);
    }

    #region Passive stage: piggy-back on the existing pull lifecycle

    private void OnPullStarted(Entity<PullerComponent> ent, ref PullStartedMessage args)
    {
        if (args.PullerUid != ent.Owner || !HasComp<MobStateComponent>(args.PulledUid))
            return;

        var grabber = EnsureComp<GrabberComponent>(ent.Owner);
        grabber.Grabbing = args.PulledUid;
        grabber.Stage = GrabStage.Passive;
        Dirty(ent.Owner, grabber);

        var grabbable = EnsureComp<GrabbableComponent>(args.PulledUid);
        grabbable.Grabber = ent.Owner;
        grabbable.Stage = GrabStage.Passive;
        Dirty(args.PulledUid, grabbable);
    }

    private void OnPullStopped(Entity<PullerComponent> ent, ref PullStoppedMessage args)
    {
        if (args.PullerUid != ent.Owner)
            return;

        RemComp<GrabberComponent>(ent.Owner);
        RemComp<GrabbableComponent>(args.PulledUid);
    }

    private void OnGrabberShutdown(Entity<GrabberComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.ThrowActionEntity is { } action)
            _actions.RemoveAction(ent.Owner, action);
    }

    private void OnGrabbableShutdown(Entity<GrabbableComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.AggressiveAlert);
        _alerts.ClearAlert(ent.Owner, ent.Comp.ChokeAlert);
    }

    #endregion

    #region Escalation: intercept combat-mode clicks on our own grab target

    private void OnAttemptMelee(Entity<GrabberComponent> ent, ref AttemptMeleeEvent args)
    {
        if (args.Cancelled || args.Target is not { } target || target != ent.Comp.Grabbing)
            return;

        if (!_combatMode.IsInCombatMode(ent.Owner))
            return;

        if (!TryComp<GrabbableComponent>(target, out var grabbableComp))
            return;

        // Always block the melee hit against our own grab target while in combat mode -
        // the click should escalate the grab, never deal weapon damage on top of it.
        args.Cancelled = true;

        if (_timing.CurTime < ent.Comp.NextEscalation)
            return;

        TryEscalate(ent, (target, grabbableComp));
    }

    private void TryEscalate(Entity<GrabberComponent> grabber, Entity<GrabbableComponent> grabbable)
    {
        var next = grabber.Comp.Stage switch
        {
            GrabStage.Passive => GrabStage.Aggressive,
            GrabStage.Aggressive => GrabStage.Choke,
            _ => grabber.Comp.Stage,
        };

        if (next == grabber.Comp.Stage)
        {
            _popup.PopupClient(Loc.GetString("grab-already-choking"), grabber, grabber);
            return;
        }

        grabber.Comp.Stage = next;
        grabber.Comp.NextEscalation = _timing.CurTime + grabber.Comp.EscalationCooldown;
        Dirty(grabber);

        grabbable.Comp.Stage = next;
        grabbable.Comp.NextChokeTick = _timing.CurTime + grabbable.Comp.ChokeTickInterval;
        Dirty(grabbable);

        if (next == GrabStage.Aggressive)
        {
            _actions.AddAction(grabber.Owner, ref grabber.Comp.ThrowActionEntity, grabber.Comp.ThrowActionId);
            _alerts.ShowAlert(grabbable.Owner, grabbable.Comp.AggressiveAlert);
        }
        else if (next == GrabStage.Choke)
        {
            _alerts.ClearAlert(grabbable.Owner, grabbable.Comp.AggressiveAlert);
            _alerts.ShowAlert(grabbable.Owner, grabbable.Comp.ChokeAlert);
        }

        var locKey = next == GrabStage.Aggressive ? "grab-escalate-aggressive" : "grab-escalate-choke";
        _popup.PopupEntity(Loc.GetString(locKey, ("target", grabbable.Owner)), grabber, grabber, PopupType.MediumCaution);
    }

    #endregion

    #region Throw (stage 2/3)

    private void OnThrowAction(Entity<GrabberComponent> ent, ref GrabThrowActionEvent args)
    {
        if (args.Handled || ent.Comp.Stage == GrabStage.Passive)
            return;

        if (ent.Comp.Grabbing is not { } target || !TryComp<PullableComponent>(target, out var pullable))
            return;

        var userPos = _transform.GetMapCoordinates(ent.Owner).Position;
        var targetPos = _transform.ToMapCoordinates(args.Target).Position;
        var direction = targetPos - userPos;

        if (direction.LengthSquared() < 0.01f)
            return;

        args.Handled = true;

        // Stopping the pull first tears down Grabber/Grabbable via OnPullStopped.
        _pulling.TryStopPull(target, pullable, ent.Owner);

        var thrown = EnsureComp<GrabThrownComponent>(target);
        thrown.Thrower = ent.Owner;
        thrown.SpawnTime = _timing.CurTime;
        Dirty(target, thrown);

        _throwing.TryThrow(target, direction.Normalized(), ent.Comp.ThrowSpeed, ent.Owner, pushbackRatio: 0f);
    }

    #endregion

    #region Knockdown-on-collision for thrown grabbed mobs

    private void OnGrabThrownCollide(Entity<GrabThrownComponent> ent, ref StartCollideEvent args)
    {
        if (!args.OurFixture.Hard || !args.OtherFixture.Hard)
            return;

        if (args.OtherEntity == ent.Comp.Thrower || !HasComp<MobStateComponent>(args.OtherEntity))
            return;

        _stun.TryKnockdown(ent.Owner, ent.Comp.KnockdownDuration, force: true);
        _stun.TryKnockdown(args.OtherEntity, ent.Comp.KnockdownDuration, force: true);

        _popup.PopupEntity(Loc.GetString("grab-throw-knockdown"), ent.Owner, PopupType.LargeCaution);

        RemCompDeferred<GrabThrownComponent>(ent.Owner);
    }

    #endregion

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        // Choke damage tick.
        var chokeQuery = EntityQueryEnumerator<GrabbableComponent>();
        while (chokeQuery.MoveNext(out var uid, out var grabbable))
        {
            if (grabbable.Stage != GrabStage.Choke || curTime < grabbable.NextChokeTick)
                continue;

            if (grabbable.Grabber is not { } grabber ||
                !TryComp<GrabberComponent>(grabber, out var grabberComp) ||
                grabberComp.Grabbing != uid)
            {
                RemComp<GrabbableComponent>(uid);
                continue;
            }

            grabbable.NextChokeTick = curTime + grabbable.ChokeTickInterval;
            Dirty(uid, grabbable);
            _damageable.TryChangeDamage(uid, grabbable.ChokeDamage, origin: grabber);
            _popup.PopupEntity(Loc.GetString("grab-choking-victim"), uid, uid, PopupType.MediumCaution);
        }

        // Safety expiry for the thrown-grab marker if it never collided with anything.
        var thrownQuery = EntityQueryEnumerator<GrabThrownComponent>();
        while (thrownQuery.MoveNext(out var uid, out var thrown))
        {
            if (curTime - thrown.SpawnTime >= thrown.Lifetime)
                RemComp<GrabThrownComponent>(uid);
        }
    }
}
