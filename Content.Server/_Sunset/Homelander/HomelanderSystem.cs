using System.Linq;
using System.Numerics;
using Content.Server.Antag;
using Content.Shared._Starlight.Medical.Body.Systems;
using Content.Shared._Starlight.Overlay.Components;
using Content.Shared._Starlight.Weapons.Hitscan.Events;
using Content.Shared._Sunset.Homelander;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Clothing.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Jittering;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.MouseRotator;
using Content.Shared.Physics;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.Standing;
using Content.Shared.Station;
using Content.Shared.Tag;
using Content.Shared.Weapons.Hitscan.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._Sunset.Homelander;

/// <summary>
/// Drives the Homelander antagonist: applies his damage resistance and flight/thermal-vision
/// abilities on selection, a real dual-beam mouse-tracking heat vision channel (see OnHeatVision),
/// and a passive fear aura that makes nearby crew jitter/stutter.
/// </summary>
public sealed class HomelanderSystem : EntitySystem
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;
    [Dependency] private readonly SharedJitteringSystem _jitter = default!;
    [Dependency] private readonly SharedStutteringSystem _stutter = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedStationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;

    private EntityQuery<MobStateComponent> _mobQuery;

    private DamageSpecifier _wallDamage = default!;

    private static readonly SpriteSpecifier HeatVisionTravel =
        new SpriteSpecifier.Rsi(new ResPath("Objects/Weapons/Guns/Projectiles/projectiles.rsi"), "beam_heavy");

    private static readonly SpriteSpecifier ThermalVisionIcon =
        new SpriteSpecifier.Rsi(new ResPath("_Sunset/Interface/Actions/homelander.rsi"), "thermal_vision");

    private static readonly SoundSpecifier LaserSound =
        new SoundPathSpecifier("/Audio/_Sunset/Homelander/laser_eye.ogg");

    private const string WallTag = "Wall";

    private const float FearRange = 4f;
    private const float FearInterval = 2.5f;

    /// <summary>How often (in seconds) the laser channel deals damage.</summary>
    private const float LaserTickInterval = 0.1f;

    /// <summary>How often (in seconds) the laser channel re-fires its visual bolt.</summary>
    private const float LaserVisualInterval = 0.1f;

    /// <summary>
    /// The client's hitscan renderer animates each bolt "growing" from the muzzle to its full
    /// length over (distance * 5000 / Speed) milliseconds (see Content.Client.Weapons.Ranged.
    /// Systems.GunSystem.FireEffect) - with the old Speed of 60 a 15-tile beam took a full 1.25
    /// SECONDS to visually extend, so every refresh landed on top of several still-growing
    /// bolts and looked like a barrage instead of one laser. A high Speed here isn't "how fast the
    /// beam travels" gameplay-wise (there's no projectile, it's an instant raycast) - it's purely
    /// tuning how fast that growth animation finishes, so it reads as an already-extended, steady
    /// beam that just gets repositioned each refresh instead of firing repeatedly.
    /// </summary>
    private const float HeatVisionSpeed = 3000f;

    /// <summary>How far forward (in the facing direction) the beam origin sits from body center, to
    /// read as coming from the face/eyes rather than the chest.</summary>
    private const float EyeForwardOffset = 0.2f;

    /// <summary>
    /// The client's hitscan renderer assumes a ~1.5-tile weapon-in-hand offset when drawing the
    /// travel beam (see Content.Client.Weapons.Ranged.Systems.GunSystem.FireEffect:
    /// RenderFlash(..., trace.Distance - 1.5f, ...)) - that's correct for a held gun's muzzle but
    /// eye lasers originate at the body itself, so short beams (close targets/walls) would end up
    /// with a negative or near-zero effective length and spam sprite-scale errors. Padding the
    /// Distance we send by the same 1.5 cancels that built-in offset back out.
    /// </summary>
    private const float TravelFlashOffsetCompensation = 1.5f;

    /// <summary>Minimum true beam length before we bother showing the visual at all.</summary>
    private const float MinVisualBeamLength = 1.5f;

    public override void Initialize()
    {
        base.Initialize();

        _mobQuery = GetEntityQuery<MobStateComponent>();
        _wallDamage = new DamageSpecifier { DamageDict = new() { { "Structural", 50 } } };

        SubscribeLocalEvent<HomelanderRuleComponent, AfterAntagEntitySelectedEvent>(OnAntagSelected);
        SubscribeLocalEvent<HomelanderComponent, MobStateChangedEvent>(OnHomelanderMobStateChanged);
        SubscribeLocalEvent<HomelanderHeatVisionEvent>(OnHeatVision);
        SubscribeLocalEvent<HomelanderComponent, BeforeDamageChangedEvent>(OnBeforeDamageChanged);
    }

    /// <summary>
    /// Full poison immunity - not just resistance. The Homelander damageModifierSet already zeroes
    /// Poison out (see Resources/Prototypes/_Sunset/Homelander/homelander.yml), but reagent damage
    /// (e.g. CompoundV/RawCompoundV, see TheBoys/reagents.yml) is dealt via HealthChange with
    /// IgnoreResistances defaulted true, which skips damage modifier sets entirely - this hook runs
    /// before that check, so it's the one spot that actually blocks poison regardless of source.
    /// </summary>
    private void OnBeforeDamageChanged(Entity<HomelanderComponent> ent, ref BeforeDamageChangedEvent args)
    {
        args.Damage.DamageDict.Remove("Poison");
    }

    /// <summary>
    /// His armor suit can't be taken off by himself while alive (SelfUnremovableClothingComponent) -
    /// once he dies it becomes lootable like anything else, so strip that marker from it.
    /// </summary>
    private void OnHomelanderMobStateChanged(EntityUid uid, HomelanderComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        if (_inventory.TryGetSlotEntity(uid, "outerClothing", out var outer))
            RemComp<SelfUnremovableClothingComponent>(outer.Value);
    }

    /// <summary>
    /// Flight and Thermal Vision are granted declaratively now (see game_rule.yml's
    /// AntagSelectionDefinition.components: `- type: Flight`, `- type: ThermalVision`) - the exact
    /// same proven-reliable mechanism Heat Vision, HyperHearing, Damageable etc. already use,
    /// instead of granting them here via EnsureComp/AddAction (that approach was silently failing to
    /// produce a visible action). ThermalVisionComponent grants its own toggle action automatically
    /// on MapInitEvent - do NOT also list ActionToggleThermal under ActionGrant, that produces two
    /// copies of the same action.
    ///
    /// 🌇Sunset🌇 - subscribed on HomelanderRuleComponent (the GameRule entity), not HomelanderComponent
    /// (the player's body): MakeAntag raises AfterAntagEntitySelectedEvent directed at the rule entity
    /// it was called with, not at the picked player - a directed subscription on HomelanderComponent
    /// would need the *rule* entity to have that component, which it never does, so it would silently
    /// never fire. That's also why his damage resistance was never actually coming from here - it was
    /// already covered by the declarative `- type: Damageable / damageModifierSetId: Homelander` in
    /// game_rule.yml, redundantly duplicated (and dead) below - and why his round-start briefing and
    /// "seize control of the station" objective were never actually being sent/granted.
    /// </summary>
    private void OnAntagSelected(Entity<HomelanderRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        var body = args.EntityUid;
        if (!HasComp<HomelanderComponent>(body))
            return;

        try
        {
            StripAndRedress(body);
            ReplaceBlood(body);

            _antag.SendBriefing(body, Loc.GetString("homelander-role-greeting"), null, null);

            if (_mind.TryGetMind(body, out var mindId, out var mind))
                _mind.TryAddObjective(mindId, mind, "HomelanderControlStationObjective");
        }
        catch (Exception e)
        {
            Log.Error($"Homelander setup threw for {ToPrettyString(body)}: {e}");
        }
    }

    /// <summary>
    /// Fully strips whatever the picked candidate was already wearing before dressing them in
    /// HomelanderGear. MakeAntag's own gear-equip step (AntagSelectionSystem.MakeAntag ->
    /// LoadoutSystem.Equip) force-replaces only the slots HomelanderGear itself specifies - anything
    /// from the player's previous job that doesn't overlap one of those slots (e.g. shoes, gloves,
    /// belt) would otherwise stay on, mixed in with the costume.
    /// </summary>
    private void StripAndRedress(EntityUid body)
    {
        var enumerator = _inventory.GetSlotEnumerator(body);
        while (enumerator.NextItem(out var item, out var slot))
        {
            if (_inventory.TryUnequip(body, slot.Name, out var removedItem, force: true))
                QueueDel(removedItem.Value);
        }

        _stationSpawning.EquipStartingGear(body, "HomelanderGear");
    }

    /// <summary>
    /// Actually swaps whatever blood the body's bloodstream solution currently holds for
    /// HomelanderBlood, instead of just pointing BloodReferenceSolution at it (which would leave his
    /// original species blood physically in the container - drawing blood with a syringe would still
    /// give you his old blood). Must run after StripAndRedress/AddComponents so the reference solution
    /// still reflects his real pre-swap blood, since ChangeBloodReagents uses it to find what to remove.
    /// </summary>
    private void ReplaceBlood(EntityUid body)
    {
        _bloodstream.ChangeBloodReagents(body, new Solution([new ReagentQuantity("HomelanderBlood", FixedPoint2.New(300))]));
    }

    /// <summary>
    /// Give Homelander's own copy of the (shared, also used by Changeling) thermal vision action a
    /// custom icon, without touching the shared action prototype other holders still use. Checked
    /// from Update() rather than a MapInitEvent subscription - SharedThermalVisionSystem already
    /// owns the one-and-only subscription slot for (ThermalVisionComponent, MapInitEvent) since
    /// that event only allows a single subscriber per component, full stop, regardless of before/
    /// after ordering (same restriction Robust enforces for ComponentStartup/ComponentInit).
    /// Polling once a tick until it succeeds sidesteps that instead of racing for the same slot.
    /// </summary>
    private void TryRetintThermalVision(EntityUid uid, HomelanderComponent comp)
    {
        if (comp.ThermalIconApplied)
            return;

        if (!TryComp<ThermalVisionComponent>(uid, out var thermal) || thermal.ActionEntity is not { } action)
            return;

        _actions.SetIcon(action, ThermalVisionIcon);
        comp.ThermalIconApplied = true;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<HomelanderComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            TryRetintThermalVision(uid, comp);

            comp.FearAccumulator += frameTime;
            if (comp.FearAccumulator >= FearInterval)
            {
                comp.FearAccumulator = 0f;

                foreach (var other in _lookup.GetEntitiesInRange(uid, FearRange))
                {
                    if (other == uid || !HasComp<HumanoidAppearanceComponent>(other))
                        continue;

                    if (_mobQuery.TryGetComponent(other, out var mob) && mob.CurrentState == MobState.Dead)
                        continue;

                    _jitter.DoJitter(other, TimeSpan.FromSeconds(2), true);
                    _stutter.DoStutter(other, TimeSpan.FromSeconds(4), true);
                }
            }

            if (!comp.LaserActive)
                continue;

            if (_timing.CurTime >= comp.LaserEndTime)
            {
                EndLaser(uid, comp);
                continue;
            }

            comp.LaserTickAccumulator += frameTime;
            comp.LaserVisualAccumulator += frameTime;

            if (comp.LaserTickAccumulator < LaserTickInterval)
                continue;

            var dt = comp.LaserTickAccumulator;
            comp.LaserTickAccumulator = 0f;

            var showVisual = comp.LaserVisualAccumulator >= LaserVisualInterval;
            if (showVisual)
                comp.LaserVisualAccumulator = 0f;

            FireLaserTick(uid, comp, dt, showVisual);
        }
    }

    /// <summary>
    /// Starts the twin-laser channel (up to Duration long), or - if it's already running - toggles
    /// it off early. The action is deliberately left off cooldown while channeling so it stays
    /// clickable for that toggle-off; the cooldown only gets applied once the channel actually ends
    /// (see EndLaser), whether that's from running out or from this early toggle.
    /// </summary>
    private void OnHeatVision(HomelanderHeatVisionEvent ev)
    {
        if (ev.Handled)
            return;

        ev.Handled = true;

        var uid = ev.Performer;
        if (!TryComp<HomelanderComponent>(uid, out var comp))
            return;

        if (comp.LaserActive)
        {
            EndLaser(uid, comp);
            return;
        }

        comp.LaserActive = true;
        comp.LaserEndTime = _timing.CurTime + ev.Duration;
        comp.LaserTickAccumulator = 0f;
        comp.LaserVisualAccumulator = LaserVisualInterval; // fire the first bolt immediately
        comp.LaserDamagePerSecond = ev.DamagePerSecond;
        comp.LaserRange = ev.Range;
        comp.LaserEyeOffset = ev.EyeOffset;
        comp.LaserLockout = ev.Lockout;
        comp.LaserActionEntity = ev.Action.Owner;

        var rotator = EnsureComp<MouseRotatorComponent>(uid);
        rotator.AngleTolerance = Angle.FromDegrees(1.0);
        Dirty(uid, rotator);

        // PlayPredicted excludes the "user" from hearing the sound, assuming the client already
        // played it locally via its own prediction - but HomelanderSystem is Content.Server-only
        // (no client-side counterpart), so that assumption is false and Homelander himself would
        // never hear his own laser. PlayPvs broadcasts to everyone, including him.
        comp.LaserSoundEntity = _audio.PlayPvs(LaserSound, uid, AudioParams.Default.WithLoop(true))?.Entity;
    }

    private void EndLaser(EntityUid uid, HomelanderComponent comp)
    {
        comp.LaserActive = false;
        RemComp<MouseRotatorComponent>(uid);

        _audio.Stop(comp.LaserSoundEntity);
        comp.LaserSoundEntity = null;

        if (comp.LaserActionEntity is { } action)
            _actions.SetCooldown(action, comp.LaserLockout);
    }

    private void FireLaserTick(EntityUid uid, HomelanderComponent comp, float dt, bool showVisual)
    {
        var gazerMap = _xform.GetMapCoordinates(uid);
        if (gazerMap.MapId == MapId.Nullspace)
            return;

        // GetWorldRotation uses the in-world convention (0 = South) that MouseRotatorComponent
        // itself sets rotations in (see Content.Client.MouseRotator.MouseRotatorSystem, which
        // builds its GoalRotation via Vector2.ToWorldAngle()) - ToWorldVec() is the matching
        // inverse. Plain ToVec() assumes the "0 = East" math convention and was firing every shot
        // 90 degrees off from wherever the mouse actually pointed.
        var facing = _xform.GetWorldRotation(uid);
        var direction = facing.ToWorldVec();
        var perpendicular = (facing + Angle.FromDegrees(90)).ToWorldVec();
        var sideOffset = perpendicular * comp.LaserEyeOffset;
        // Push the origin slightly forward, roughly to face/eye height instead of dead center.
        var forwardOffset = direction * EyeForwardOffset;
        var eyeCenter = gazerMap.Offset(forwardOffset);

        FireBeam(uid, eyeCenter.Offset(sideOffset), direction, comp.LaserRange, comp.LaserDamagePerSecond, dt, showVisual);
        FireBeam(uid, eyeCenter.Offset(-sideOffset), direction, comp.LaserRange, comp.LaserDamagePerSecond, dt, showVisual);
    }

    /// <summary>
    /// Internal (not private) so TheBoysPowersSystem can reuse this exact raycasting/damage logic for
    /// Butcher's own laser-eyes power instead of duplicating it - see that system's OnButcherLaserEyes.
    /// </summary>
    internal void FireBeam(EntityUid uid, MapCoordinates origin, Vector2 direction, float range, DamageSpecifier damagePerSecond, float dt, bool showVisual)
    {
        var ray = new CollisionRay(origin.Position, direction, (int) CollisionGroup.BulletImpassable);
        var results = _physics
            .IntersectRayWithPredicate(origin.MapId, ray, range, e => e == uid, false)
            .ToList();

        EntityUid? stoppedBy = null;
        var beamLength = range;

        var tickDamage = damagePerSecond * dt;

        foreach (var result in results)
        {
            var candidate = result.HitEntity;

            if (_mobQuery.HasComponent(candidate))
            {
                // Prone bodies don't block the beam - burn them and keep going.
                if (_standing.IsDown(candidate))
                {
                    _damageable.TryChangeDamage(candidate, tickDamage, origin: uid);
                    continue;
                }

                stoppedBy = candidate;
                beamLength = result.Distance;
                break;
            }

            // Anything else solid enough to be hit by the ray (wall, dense structure) stops it.
            stoppedBy = candidate;
            beamLength = result.Distance;
            break;
        }

        if (stoppedBy != null)
        {
            if (_mobQuery.HasComponent(stoppedBy.Value))
                _damageable.TryChangeDamage(stoppedBy.Value, tickDamage, origin: uid);
            else if (_tag.HasTag(stoppedBy.Value, WallTag))
                _damageable.TryChangeDamage(stoppedBy.Value, _wallDamage * dt, ignoreResistances: true, origin: uid);
        }

        // Skip degenerate near-zero-length beams entirely - the client's sprite scaling can't
        // handle a near-zero distance and spams "very small values" errors.
        if (showVisual && beamLength >= MinVisualBeamLength)
            FireHeatVisionEffect(uid, origin, direction, beamLength);
    }

    private void FireHeatVisionEffect(EntityUid uid, MapCoordinates origin, Vector2 direction, float distance)
    {
        var mapUid = Transform(uid).MapUid;
        if (mapUid == null)
            return;

        var fromCoords = new EntityCoordinates(mapUid.Value, origin.Position);
        var angle = direction.ToAngle();

        // The coordinates below use the real distance; only the Distance field (which the client
        // uses purely for its own scale/timing math) gets the padding - see
        // TravelFlashOffsetCompensation's doc comment.
        var trace = new HitscanTrace
        {
            Angle = angle,
            Distance = distance + TravelFlashOffsetCompensation,
            MuzzleCoordinates = distance > 1f ? GetNetCoordinates(fromCoords.Offset(direction / 2)) : null,
            TravelCoordinates = distance > 1f ? GetNetCoordinates(fromCoords.Offset(direction * (distance + 0.5f) / 2)) : null,
            ImpactCoordinates = GetNetCoordinates(fromCoords.Offset(direction * distance)),
        };

        // Deliberately no MuzzleFlash/ImpactFlash - those would re-flash at the eye/target every
        // single refresh, which reads as strobing/spam. Just the travel beam itself, redrawn at
        // (near-instant, see HeatVisionSpeed) full length each refresh, looks like one steady laser.
        var hitscanEvent = new SharedGunSystem.HitscanEvent
        {
            TravelFlash = HeatVisionTravel,
            Speed = HeatVisionSpeed,
            Traces = new List<HitscanTrace> { trace },
        };

        RaiseNetworkEvent(hitscanEvent, Filter.Pvs(fromCoords, entityMan: EntityManager));
    }
}
