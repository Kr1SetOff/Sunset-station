using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared._Sunset.Grab.Components;
using Content.Shared._Sunset.Grab.Events;
using Robust.Shared.Physics.Events;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Sunset.Grab.Systems;

/// <summary>
/// Drives the three-stage grab escalation (Passive -&gt; Aggressive -&gt; Choke) that sits on top of the
/// vanilla pulling system. Passive is just a normal pull started via Ctrl+Click; re-clicking (Ctrl+Click)
/// your own grab target escalates the stage instead of releasing the pull. At Aggressive/Choke, throwing
/// works through the vanilla "throw item in hand" keybind (Ctrl+Q) by substituting the grabbed mob for
/// the pull's placeholder virtual item.
/// </summary>
public sealed partial class SharedGrabSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PullerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<PullerComponent, PullStoppedMessage>(OnPullStopped);

        SubscribeLocalEvent<GrabbableComponent, GrabClickAttemptEvent>(OnGrabClickAttempt);
        SubscribeLocalEvent<GrabbableComponent, ComponentShutdown>(OnGrabbableShutdown);

        SubscribeLocalEvent<GrabberComponent, BeforeThrowEvent>(OnBeforeThrow);
        SubscribeLocalEvent<GrabberComponent, ComponentShutdown>(OnGrabberShutdown);

        SubscribeLocalEvent<GrabThrownComponent, StartCollideEvent>(OnGrabThrownCollide);
        SubscribeLocalEvent<GrabThrownComponent, LandEvent>(OnGrabThrownLand);
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

        // The very first grab counts as a "Grab" combo input too, not just later escalations.
        var escalatedEv = new GrabEscalatedEvent(args.PulledUid, GrabStage.Passive);
        RaiseLocalEvent(ent.Owner, ref escalatedEv);
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
        // Free up the "second hand" used to choke, regardless of what tore the grab down.
        if (ent.Comp.ChokeVirtualItem is { } chokeItem)
            _hands.TryDrop(ent.Owner, chokeItem);
    }

    private void OnGrabbableShutdown(Entity<GrabbableComponent> ent, ref ComponentShutdown args)
    {
        _alerts.ClearAlert(ent.Owner, ent.Comp.AggressiveAlert);
        _alerts.ClearAlert(ent.Owner, ent.Comp.ChokeAlert);
    }

    #endregion

    #region Escalation: Ctrl+Click on our own grab target

    private void OnGrabClickAttempt(Entity<GrabbableComponent> ent, ref GrabClickAttemptEvent args)
    {
        if (ent.Comp.Grabber != args.User)
            return; // not our grab target - let the vanilla pull toggle run as normal

        // This is our own grab target - never let the click fall through to the vanilla
        // toggle-pull (which would release it); escalate instead, once cooldown allows.
        args.Handled = true;

        if (!TryComp<GrabberComponent>(args.User, out var grabberComp) || grabberComp.Grabbing != ent.Owner)
            return;

        if (_timing.CurTime < grabberComp.NextEscalation)
            return;

        TryEscalate((args.User, grabberComp), ent);
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
            // Throttle this too, otherwise spam-clicking at max stage spams the popup as well.
            grabber.Comp.NextEscalation = _timing.CurTime + grabber.Comp.EscalationCooldown;
            Dirty(grabber);
            return;
        }

        if (next == GrabStage.Choke)
        {
            // Choking takes both hands - bail out if there isn't a second free one.
            if (!_virtualItem.TrySpawnVirtualItemInHand(grabbable.Owner, grabber.Owner, out var chokeItem))
            {
                _popup.PopupClient(Loc.GetString("grab-choke-needs-hand"), grabber, grabber);
                grabber.Comp.NextEscalation = _timing.CurTime + grabber.Comp.EscalationCooldown;
                Dirty(grabber);
                return;
            }

            grabber.Comp.ChokeVirtualItem = chokeItem;
        }

        grabber.Comp.Stage = next;
        grabber.Comp.NextEscalation = _timing.CurTime + grabber.Comp.EscalationCooldown;
        Dirty(grabber);

        grabbable.Comp.Stage = next;
        grabbable.Comp.NextChokeTick = _timing.CurTime +
            (next == GrabStage.Choke ? grabbable.Comp.ChokeStartupDelay : grabbable.Comp.ChokeTickInterval);
        Dirty(grabbable);

        if (next == GrabStage.Aggressive)
        {
            _alerts.ShowAlert(grabbable.Owner, grabbable.Comp.AggressiveAlert);
        }
        else if (next == GrabStage.Choke)
        {
            _alerts.ClearAlert(grabbable.Owner, grabbable.Comp.AggressiveAlert);
            _alerts.ShowAlert(grabbable.Owner, grabbable.Comp.ChokeAlert);
        }

        var locKey = next == GrabStage.Aggressive ? "grab-escalate-aggressive" : "grab-escalate-choke";
        _popup.PopupEntity(Loc.GetString(locKey, ("target", grabbable.Owner)), grabber, grabber, PopupType.MediumCaution);

        var escalatedEv = new GrabEscalatedEvent(grabbable.Owner, next);
        RaiseLocalEvent(grabber.Owner, ref escalatedEv);
    }

    #endregion

    #region Throw (stage 2/3): piggy-back on the vanilla "throw item in hand" keybind

    private void OnBeforeThrow(Entity<GrabberComponent> ent, ref BeforeThrowEvent args)
    {
        if (ent.Comp.Stage == GrabStage.Passive || ent.Comp.Grabbing is not { } target)
            return;

        // Only hijack the throw if what's actually being thrown is our grab's hand placeholder.
        if (!TryComp<VirtualItemComponent>(args.ItemUid, out var virtualItem) || virtualItem.BlockingEntity != target)
            return;

        if (!TryComp<PullableComponent>(target, out var pullable))
            return;

        // Release the joint first so it doesn't fight the throw impulse; this also tears down
        // Grabber/Grabbable/choke-hand state via OnPullStopped/OnGrabberShutdown.
        _pulling.TryStopPull(target, pullable, ent.Owner);

        var thrown = EnsureComp<GrabThrownComponent>(target);
        thrown.Thrower = ent.Owner;
        thrown.SpawnTime = _timing.CurTime;

        // Substitute the grabbed mob for the placeholder - vanilla code throws whatever ItemUid is now.
        args.ItemUid = target;
        // A mob is much heavier than a held item - it shouldn't fly nearly as far/fast. Vanilla throws
        // "compensate friction" (i.e. always travel the full clamped throw range) unless the thrown
        // entity has LandAtCursorComponent, in which case distance is governed by raw ThrowSpeed instead -
        // that's what we actually want here so our speed multiplier below has a real effect.
        EnsureComp<LandAtCursorComponent>(target);
        args.ThrowSpeed *= ent.Comp.ThrowSpeedMultiplier;
    }

    #endregion

    #region Knockdown-on-collision for thrown grabbed mobs

    private void OnGrabThrownCollide(Entity<GrabThrownComponent> ent, ref StartCollideEvent args)
    {
        // Match ThrownItemSystem's own collision check: the thrown entity's dedicated throw-fixture
        // is intentionally non-hard, so only the other party's fixture needs to be hard.
        if (!args.OtherFixture.Hard)
            return;

        if (args.OtherEntity == ent.Comp.Thrower || !HasComp<MobStateComponent>(args.OtherEntity))
            return;

        _stun.TryKnockdown(ent.Owner, ent.Comp.KnockdownDuration, force: true);
        _stun.TryKnockdown(args.OtherEntity, ent.Comp.KnockdownDuration, force: true);

        var amount = _random.NextFloat((float) ent.Comp.CollisionDamageMin, (float) ent.Comp.CollisionDamageMax);
        var damage = new DamageSpecifier();
        damage.DamageDict["Blunt"] = FixedPoint2.New(amount);
        _damageable.TryChangeDamage(ent.Owner, damage);
        _damageable.TryChangeDamage(args.OtherEntity, damage);

        _popup.PopupEntity(Loc.GetString("grab-throw-knockdown"), ent.Owner, PopupType.LargeCaution);

        RemCompDeferred<GrabThrownComponent>(ent.Owner);
        RemCompDeferred<LandAtCursorComponent>(ent.Owner);
    }

    /// <summary>
    /// A thrown mob should crash to the floor even if it never hit anyone - otherwise a throw that
    /// lands in open space has no effect at all besides the travel itself.
    /// </summary>
    private void OnGrabThrownLand(Entity<GrabThrownComponent> ent, ref LandEvent args)
    {
        _stun.TryKnockdown(ent.Owner, ent.Comp.KnockdownDuration, force: true);

        RemCompDeferred<GrabThrownComponent>(ent.Owner);
        RemCompDeferred<LandAtCursorComponent>(ent.Owner);
    }

    #endregion

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        // Safety: if the grabber and their grab target ever end up on different maps (teleport,
        // disposal pipe, shuttle FTL, etc.) the underlying vanilla pull's distance joint would try to
        // link bodies across maps, which is a fatal engine-level assert during client prediction.
        // Release the pull the instant that happens instead of letting it linger into a bad joint.
        var grabberQuery = EntityQueryEnumerator<GrabberComponent>();
        while (grabberQuery.MoveNext(out var grabberUid, out var grabber))
        {
            if (grabber.Grabbing is not { } grabbed)
                continue;

            if (_transform.GetMapId(grabberUid) == _transform.GetMapId(grabbed))
                continue;

            if (TryComp<PullableComponent>(grabbed, out var pullable))
                _pulling.TryStopPull(grabbed, pullable, grabberUid);
        }

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

            var amount = _random.NextFloat((float) grabbable.ChokeDamageMin, (float) grabbable.ChokeDamageMax);
            var damage = new DamageSpecifier();
            damage.DamageDict["Asphyxiation"] = FixedPoint2.New(amount);
            // No popup here on purpose - this ticks every second while choked, a chat message each
            // time would just be spam. The choke alert + falling health already tell the story.
            _damageable.TryChangeDamage(uid, damage, origin: grabber);
        }

        // Safety expiry for the thrown-grab marker if it never collided with anything.
        var thrownQuery = EntityQueryEnumerator<GrabThrownComponent>();
        while (thrownQuery.MoveNext(out var uid, out var thrown))
        {
            if (curTime - thrown.SpawnTime >= thrown.Lifetime)
            {
                RemComp<GrabThrownComponent>(uid);
                RemComp<LandAtCursorComponent>(uid);
            }
        }
    }
}
