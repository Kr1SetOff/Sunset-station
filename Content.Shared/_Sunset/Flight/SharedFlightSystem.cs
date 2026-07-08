using Content.Shared.Actions;
using Content.Shared.Bed.Sleep;
using Content.Shared.Climbing.Events;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Gravity;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Components;
using Content.Shared.Inventory.VirtualItem;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.RepulseAttract;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Shared.Zombies;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._Sunset.Flight;

/// <summary>
/// Drives the FlightComponent (ported from Goob-Station's Harpy flight mechanic - see
/// FlightComponent.cs for the port notes). Weightless movement toggle (overrides station gravity
/// while airborne, same mechanism as anti-gravity clothing - see AntiGravityClothingSystem),
/// collision mask changes while airborne, hand-blocking, a continuous stamina drain while
/// flying, and a landing shockwave reusing the Wizard Repulse spell's RepulseAttractSystem at
/// half its speed.
/// </summary>
public abstract class SharedFlightSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly SharedVirtualItemSystem _virtualItem = default!;
    [Dependency] private readonly SharedStaminaSystem _staminaSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popupSystem = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedGravitySystem _gravity = default!;
    [Dependency] private readonly RepulseAttractSystem _repulseAttract = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly PacificationSystem _pacification = default!;

    /// <summary>
    /// Half of the Wizard Repulse spell's tuned speed (ActionRepulse uses speed: 10) - see
    /// Resources/Prototypes/Magic/repulse_spell.yml.
    /// </summary>
    private const float LandingRepulseSpeed = 5f;
    private const float LandingRepulseRange = 5f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FlightComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<FlightComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<FlightComponent, ToggleFlightEvent>(OnToggleFlight);
        SubscribeLocalEvent<FlightComponent, RefreshWeightlessModifiersEvent>(OnRefreshWeightlessMoveSpeed);
        SubscribeLocalEvent<FlightComponent, IsWeightlessEvent>(OnIsWeightless);
        SubscribeLocalEvent<FlightComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<CuffableComponent, FlightAttemptEvent>(OnCuffableFlightAttempt);
        SubscribeLocalEvent<StandingStateComponent, FlightAttemptEvent>(OnStandingStateFlightAttempt);
        SubscribeLocalEvent<ZombieComponent, FlightAttemptEvent>(OnZombieFlightAttempt);
        SubscribeLocalEvent<FlightComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
        SubscribeLocalEvent<FlightComponent, EntityZombifiedEvent>(OnFlightDisablingEvent);
        SubscribeLocalEvent<FlightComponent, KnockedDownEvent>(OnFlightDisablingEvent);
        SubscribeLocalEvent<FlightComponent, StunnedEvent>(OnFlightDisablingEvent);
        SubscribeLocalEvent<FlightComponent, DownedEvent>(OnDowned);
        SubscribeLocalEvent<FlightComponent, SleepStateChangedEvent>(OnSleep);
        SubscribeLocalEvent<FlightComponent, AttemptClimbEvent>(OnAttemptClimb);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<FlightComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (!component.On)
                continue;

            if (component.FlapSound != null)
            {
                component.TimeUntilFlap -= frameTime;

                if (component.TimeUntilFlap <= 0f)
                {
                    // PlayPredicted excludes "user" from hearing the sound, assuming the client
                    // already played it locally via its own prediction - but there's no client-side
                    // FlightSystem counterpart in this fork, so that assumption is false and the
                    // flying entity itself would never hear its own flap sound. PlayPvs broadcasts
                    // to everyone, including them.
                    _audio.PlayPvs(component.FlapSound, uid);
                    component.TimeUntilFlap = component.FlapInterval;
                }
            }

            // No keyed continuous-drain source system on this fork's stamina system, so we just
            // apply the per-second drain directly every tick while flying.
            _staminaSystem.TakeStaminaDamage(uid, component.StaminaDrainRate * frameTime, visual: false);
        }
    }

    #region Core Functions

    private void OnStartup(EntityUid uid, FlightComponent component, ComponentStartup args)
    {
        _actionsSystem.AddAction(uid, ref component.ToggleActionEntity, component.ToggleAction);
    }

    private void OnShutdown(EntityUid uid, FlightComponent component, ComponentShutdown args)
    {
        _actionsSystem.RemoveAction(uid, component.ToggleActionEntity);
        if (!TerminatingOrDeleted(uid))
            ToggleActive(uid, false, component);
    }

    public void ToggleActive(EntityUid uid, bool active, FlightComponent component, bool gracefulStop = true)
    {
        var wasOn = component.On;

        component.On = active;
        component.TimeUntilFlap = 0f;
        _actionsSystem.SetToggled(component.ToggleActionEntity, component.On);
        RaiseLocalEvent(uid, new FlightEvent(uid, component.On));
        _gravity.RefreshWeightless(uid, active);
        _movementSpeed.RefreshWeightlessModifiers(uid);
        ToggleCollisionMasks(uid, component);
        UpdateHands(uid, active);
        UpdateCombatBlock(uid, component, active);

        if (component.CanFail && !gracefulStop)
            _damageable.TryChangeDamage(uid, component.FailDamage);

        // Landing shockwave + sound: only on an actual on->off transition, not on a failed/no-op toggle.
        if (wasOn && !active)
        {
            var landingPos = _xform.GetMapCoordinates(uid);
            _repulseAttract.TryRepulseAttract(landingPos, uid, LandingRepulseSpeed, LandingRepulseRange);
            // See the flap sound comment above - no client-side FlightSystem exists, so PlayPredicted
            // would silently mute this for the landing entity itself.
            _audio.PlayPvs(component.LandingSound, uid);
        }

        Dirty(uid, component);
    }

    private void OnToggleFlight(EntityUid uid, FlightComponent component, ToggleFlightEvent args)
    {
        if (!component.On
            && !CanFly(uid, component))
            return;

        ToggleActive(uid, !component.On, component);
    }

    private void ToggleCollisionMasks(EntityUid uid, FlightComponent component)
    {
        if (!component.ChangeCollisionMasks)
            return;

        if (component.On)
            DisableCollisionMasks(uid, component);
        else
            EnableCollisionMasks(uid, component);
    }

    private void DisableCollisionMasks(EntityUid uid, FlightComponent component)
    {
        if (!component.On)
            return;

        if (TryComp(uid, out FixturesComponent? fixtureComponent))
        {
            foreach (var (key, fixture) in fixtureComponent.Fixtures)
            {
                var newMask = (fixture.CollisionMask
                    & (int) ~CollisionGroup.HighImpassable
                    & (int) ~CollisionGroup.MidImpassable)
                    | (int) CollisionGroup.InteractImpassable;

                if (fixture.CollisionMask == newMask)
                    continue;

                component.ChangedFixtures.Add((key, fixture.CollisionMask));
                _physics.SetCollisionMask(uid,
                    key,
                    fixture,
                    newMask,
                    manager: fixtureComponent);
            }
        }
    }

    private void EnableCollisionMasks(EntityUid uid, FlightComponent component)
    {
        if (component.On)
            return;

        if (TryComp(uid, out FixturesComponent? fixtureComponent))
            foreach (var (key, originalMask) in component.ChangedFixtures)
                if (fixtureComponent.Fixtures.TryGetValue(key, out var fixture))
                    _physics.SetCollisionMask(uid, key, fixture, originalMask, fixtureComponent);

        component.ChangedFixtures.Clear();
    }

    private void UpdateHands(EntityUid uid, bool flying)
    {
        if (!TryComp<HandsComponent>(uid, out var handsComponent))
            return;

        if (flying)
            BlockHands(uid, handsComponent);
        else
            FreeHands(uid);
    }

    private void BlockHands(EntityUid uid, HandsComponent handsComponent)
    {
        var freeHands = 0;
        foreach (var hand in _hands.EnumerateHands((uid, handsComponent)))
        {
            if (!_hands.TryGetHeldItem((uid, handsComponent), hand, out var held))
            {
                freeHands++;
                continue;
            }

            if (HasComp<UnremoveableComponent>(held) && held != uid)
                continue;

            _hands.DoDrop((uid, handsComponent), hand);
            freeHands++;
            if (freeHands == 2)
                break;
        }

        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem1))
            EnsureComp<UnremoveableComponent>(virtItem1.Value);

        if (_virtualItem.TrySpawnVirtualItemInHand(uid, uid, out var virtItem2))
            EnsureComp<UnremoveableComponent>(virtItem2.Value);
    }

    private void FreeHands(EntityUid uid) => _virtualItem.DeleteInHandsMatching(uid, uid);

    /// <summary>
    /// Flying means holding yourself with both hands (see UpdateHands/BlockHands above), so you
    /// can't throw a punch while airborne - block all combat for the duration, same as the
    /// generic Pacified status effect used elsewhere (e.g. the Pacifism gene). Only adds/removes
    /// the component if flight itself is what put it there - if the entity was already Pacified for
    /// an unrelated reason (e.g. a Pacifism gene) before takeoff, landing must not silently lift that.
    /// </summary>
    private void UpdateCombatBlock(EntityUid uid, FlightComponent component, bool flying)
    {
        if (flying)
        {
            component.PacifiedByFlight = !HasComp<PacifiedComponent>(uid);
            _pacification.SetFullyPacified(uid, true);
        }
        else if (component.PacifiedByFlight)
        {
            component.PacifiedByFlight = false;
            _pacification.SetFullyPacified(uid, false);
        }
    }

    private void OnRefreshWeightlessMoveSpeed(EntityUid uid, FlightComponent component, ref RefreshWeightlessModifiersEvent args)
    {
        if (!component.On)
            return;

        args.ModifyAcceleration(component.SpeedModifier);
        args.ModifyFriction(component.FrictionModifier, component.FrictionNoInputModifier);
    }

    private void OnIsWeightless(EntityUid uid, FlightComponent component, ref IsWeightlessEvent args)
    {
        if (!component.On || args.Handled)
            return;

        // Overrides station gravity while flying - same mechanism as anti-gravity clothing
        // (AntiGravityClothingSystem) - so RefreshWeightlessModifiersEvent (which is what our own
        // speed/friction modifiers hook into) actually fires even on a grid with gravity enabled.
        args.Handled = true;
        args.IsWeightless = true;
    }

    private void OnBeforeStaminaDamage(EntityUid uid, FlightComponent component, ref BeforeStaminaDamageEvent args)
    {
        if (!component.On
            || args.Value > 0)
            return;

        args.Value *= component.StaminaRegenMultiplier;
    }

    #endregion

    #region Conditionals

    private bool CanFly(EntityUid uid, FlightComponent component)
    {
        var ev = new FlightAttemptEvent();
        RaiseLocalEvent(uid, ref ev);

        return !ev.Cancelled;
    }

    private void OnCuffableFlightAttempt(EntityUid uid, CuffableComponent component, ref FlightAttemptEvent args)
    {
        if (component.CanStillInteract)
            return;

        _popupSystem.PopupClient(Loc.GetString("no-flight-while-restrained"), uid, uid, PopupType.Medium);
        args.Cancel();
    }

    private void OnZombieFlightAttempt(EntityUid uid, ZombieComponent component, ref FlightAttemptEvent args)
    {
        _popupSystem.PopupClient(Loc.GetString("no-flight-while-zombified"), uid, uid, PopupType.Medium);
        args.Cancel();
    }

    private void OnStandingStateFlightAttempt(EntityUid uid, StandingStateComponent component, ref FlightAttemptEvent args)
    {
        if (!_standing.IsDown((uid, component)))
            return;

        _popupSystem.PopupClient(Loc.GetString("no-flight-while-lying"), uid, uid, PopupType.Medium);
        args.Cancel();
    }

    #endregion

    #region Misc.Handlers

    private void OnMobStateChangedEvent(EntityUid uid, FlightComponent component, MobStateChangedEvent args)
    {
        if (!component.On
            || args.NewMobState is MobState.Critical or MobState.Dead)
            return;

        ToggleActive(args.Target, false, component, gracefulStop: false);
    }

    private void OnSleep(EntityUid uid, FlightComponent component, ref SleepStateChangedEvent args)
    {
        if (!component.On
            || !args.FellAsleep)
            return;

        ToggleActive(uid, false, component, gracefulStop: false);
    }

    private void OnDowned(EntityUid uid, FlightComponent component, DownedEvent args)
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component, gracefulStop: false);
        RaiseNetworkEvent(new ToggleFlightVisualsEvent(GetNetEntity(uid), false));
    }

    private void OnFlightDisablingEvent<T>(EntityUid uid, FlightComponent component, ref T args) where T : notnull
    {
        if (!component.On)
            return;

        ToggleActive(uid, false, component, gracefulStop: false);
    }

    private void OnAttemptClimb(EntityUid uid, FlightComponent component, ref AttemptClimbEvent args)
    {
        if (!component.On)
            return;

        args.Cancelled = true;
    }

    #endregion
}
