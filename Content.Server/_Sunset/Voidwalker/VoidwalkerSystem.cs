using System.Linq;
using Content.Server.Atmos.Components;
using Content.Server.Atmos.EntitySystems;
using Content.Server.Station.Components;
using Content.Shared._Starlight.Medical.Body.Events;
using Content.Shared._Starlight.Medical.Body.Part;
using Content.Shared._Sunset.Voidwalker;
using Content.Shared.Body.Systems;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.FixedPoint;
using Content.Shared.Damage.Systems;
using Content.Server.Electrocution;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Medical.SuitSensor;
using Content.Shared.Medical.SuitSensors;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Content.Shared.Random.Helpers;
using Content.Shared.Speech.Muting;
using Content.Shared.Station.Components;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Collections;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - see VoidwalkerComponent's doc comment for scope notes.
/// </summary>
public sealed class VoidwalkerSystem : SharedVoidwalkerSystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedStaminaSystem _stamina = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphere = default!;
    [Dependency] private readonly SharedSuitSensorSystem _suitSensor = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly ElectrocutionSystem _electrocution = default!;

    private static readonly string[] TelepathyPhrases =
    {
        "voidwalker-telepathy-phrase-watching",
        "voidwalker-telepathy-phrase-cold",
        "voidwalker-telepathy-phrase-glass",
        "voidwalker-telepathy-phrase-come",
        "voidwalker-telepathy-phrase-truth",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerUnsettleActionEvent>(OnUnsettle);
        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerTelepathyActionEvent>(OnTelepathy);
        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerKidnapActionEvent>(OnKidnap);
        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerGlassifyActionEvent>(OnGlassify);
        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerUnsettleDoAfterEvent>(OnUnsettleDoAfter);
        SubscribeLocalEvent<VoidwalkerComponent, VoidwalkerKidnapDoAfterEvent>(OnKidnapDoAfter);
        SubscribeLocalEvent<VoidwalkerComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<VoidTumorComponent, OrganRemovedFromBodyEvent>(OnTumorRemoved);
        SubscribeLocalEvent<VoidwalkerComponent, PullStartedMessage>(OnPullStarted);
        SubscribeLocalEvent<VoidwalkerComponent, PullStoppedMessage>(OnPullStopped);
    }

    /// <summary>
    /// Heavily damp whatever the Voidwalker grabs so it stops when he stops instead of orbiting
    /// the loose pull joint. Both events are raised at puller AND pulled, so filter to "we are
    /// the puller".
    /// </summary>
    private void OnPullStarted(Entity<VoidwalkerComponent> ent, ref PullStartedMessage args)
    {
        if (args.PullerUid != ent.Owner || !TryComp<PhysicsComponent>(args.PulledUid, out var physics))
            return;

        var grip = EnsureComp<VoidGripComponent>(args.PulledUid);
        grip.OldLinearDamping = physics.LinearDamping;
        _physics.SetLinearDamping(args.PulledUid, physics, 8f);
    }

    private void OnPullStopped(Entity<VoidwalkerComponent> ent, ref PullStoppedMessage args)
    {
        if (args.PullerUid != ent.Owner || !TryComp<VoidGripComponent>(args.PulledUid, out var grip))
            return;

        if (TryComp<PhysicsComponent>(args.PulledUid, out var physics))
            _physics.SetLinearDamping(args.PulledUid, physics, grip.OldLinearDamping);

        RemComp<VoidGripComponent>(args.PulledUid);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        // Space regeneration: the void slowly mends any voidwalker drifting off-grid.
        var walkerQuery = EntityQueryEnumerator<VoidwalkerComponent, DamageableComponent>();
        while (walkerQuery.MoveNext(out var walkerUid, out var walker, out var damageable))
        {
            if (now < walker.NextSpaceRegenTime)
                continue;

            walker.NextSpaceRegenTime = now + walker.SpaceRegenInterval;

            if (Transform(walkerUid).GridUid != null
                || _mobState.IsDead(walkerUid)
                || damageable.TotalDamage <= FixedPoint2.Zero)
                continue;

            _damageable.TryChangeDamage(walkerUid, walker.SpaceRegen, ignoreResistances: true, interruptsDoAfters: false);
        }

        var query = EntityQueryEnumerator<VoidedComponent>();
        while (query.MoveNext(out var uid, out var voided))
        {
            if (now < voided.EndTime)
                continue;

            RemComp<VoidedComponent>(uid);
            RemComp<MutedComponent>(uid);
            RemComp<PacifiedComponent>(uid);
            _popup.PopupEntity(Loc.GetString("voidwalker-voided-fades"), uid, uid);
        }

        var glassQuery = EntityQueryEnumerator<VoidGlassComponent>();
        while (glassQuery.MoveNext(out var uid, out var glass))
        {
            if (now < glass.EndTime)
                continue;

            RemComp<VoidGlassComponent>(uid);
            _physics.SetCanCollide(uid, true);
        }

        var tumorQuery = EntityQueryEnumerator<VoidTumorComponent>();
        while (tumorQuery.MoveNext(out var tumorUid, out var tumor))
        {
            if (now >= tumor.EndTime)
            {
                FinishTumor(tumorUid, tumor);
                continue;
            }

            if (now < tumor.NextEffectTime)
                continue;

            var progress = Math.Clamp((float) ((now - tumor.StartTime) / (tumor.EndTime - tumor.StartTime)), 0f, 1f);
            // Ticks get more frequent and hit harder the closer the tumor is to completing.
            tumor.NextEffectTime = now + TimeSpan.FromSeconds(MathHelper.Lerp(20f, 5f, progress));
            _damageable.TryChangeDamage(tumor.Victim, new DamageSpecifier { DamageDict = { { "Cellular", MathHelper.Lerp(1f, 6f, progress) } } }, true);
        }
    }

    /// <summary>
    /// Sunset: records which station a Voidwalker was spawned near (called by
    /// VoidwalkerSpawnRuleSystem, which can't write VoidwalkerComponent directly - it's Access-
    /// restricted to SharedVoidwalkerSystem) so OnVoidwalkerMindAdded can point a new player the
    /// right way once they take over.
    /// </summary>
    public void SetSpawnStation(EntityUid voidwalker, EntityUid station)
    {
        if (TryComp<VoidwalkerComponent>(voidwalker, out var comp))
            comp.SpawnStation = station;
    }

    #region Unsettle

    private void OnUnsettle(Entity<VoidwalkerComponent> ent, ref VoidwalkerUnsettleActionEvent args)
    {
        if (args.Handled || !TryComp<MobStateComponent>(args.Target, out _))
            return;

        if (!_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 9f))
        {
            _popup.PopupClient(Loc.GetString("voidwalker-unsettle-no-los"), ent, ent);
            return;
        }

        args.Handled = true;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(6), new VoidwalkerUnsettleDoAfterEvent(), ent, args.Target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
            RequireCanInteract = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnUnsettleDoAfter(Entity<VoidwalkerComponent> ent, ref VoidwalkerUnsettleDoAfterEvent args)
    {
        if (args.Cancelled || args.Args.Target is not { } target)
            return;

        _stun.TryAddParalyzeDuration(target, TimeSpan.FromSeconds(2));
        _stamina.TryTakeStamina(target, 80f);

        _popup.PopupEntity(Loc.GetString("voidwalker-unsettle-success-self"), target, target, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("voidwalker-unsettle-success-others", ("target", target)), target, Filter.PvsExcept(target), true, PopupType.MediumCaution);
    }

    #endregion

    #region Telepathy

    private void OnTelepathy(Entity<VoidwalkerComponent> ent, ref VoidwalkerTelepathyActionEvent args)
    {
        if (args.Handled || !HasComp<MobStateComponent>(args.Target))
            return;

        args.Handled = true;

        var phrase = Loc.GetString(_random.Pick(TelepathyPhrases));
        _popup.PopupEntity(Loc.GetString("voidwalker-telepathy-received", ("phrase", phrase)), args.Target, args.Target, PopupType.Medium);
        _popup.PopupEntity(Loc.GetString("voidwalker-telepathy-sent"), ent, ent);
    }

    #endregion

    #region Kidnap

    private void OnKidnap(Entity<VoidwalkerComponent> ent, ref VoidwalkerKidnapActionEvent args)
    {
        if (args.Handled || !TryComp<MobStateComponent>(args.Target, out var mobState))
            return;

        // Sunset: dead victims are fair game too - the void restores them (see OnKidnapDoAfter's
        // full heal), so a kidnap doubles as a twisted resurrection.
        if (!_mobState.IsIncapacitated(args.Target, mobState))
        {
            _popup.PopupClient(Loc.GetString("voidwalker-kidnap-conscious"), ent, ent);
            return;
        }

        if (HasComp<VoidedComponent>(args.Target))
        {
            _popup.PopupClient(Loc.GetString("voidwalker-kidnap-already-voided"), ent, ent);
            return;
        }

        if (Transform(args.Target).GridUid != null)
        {
            _popup.PopupClient(Loc.GetString("voidwalker-kidnap-not-in-space"), ent, ent);
            return;
        }

        if (!_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 1.5f))
        {
            _popup.PopupClient(Loc.GetString("voidwalker-kidnap-too-far"), ent, ent);
            return;
        }

        args.Handled = true;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.KidnapTime, new VoidwalkerKidnapDoAfterEvent(), ent, args.Target)
        {
            // Movement doesn't interrupt the kidnap - both the voidwalker and the (drifting,
            // dragged) target float freely in space. The default 1.5-tile DistanceThreshold still
            // applies, so the channel only breaks if they actually separate.
            BreakOnMove = false,
            NeedHand = false,
            RequireCanInteract = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnKidnapDoAfter(Entity<VoidwalkerComponent> ent, ref VoidwalkerKidnapDoAfterEvent args)
    {
        if (args.Cancelled || args.Args.Target is not { } target)
            return;

        // Sunset: the void fully restores its prize - crit or outright dead, the victim comes back
        // whole (raised BEFORE the curse components go on, so the rejuvenate can't clear them).
        RaiseLocalEvent(target, new RejuvenateEvent());

        EnsureComp<MutedComponent>(target);
        EnsureComp<PacifiedComponent>(target);

        var voided = EnsureComp<VoidedComponent>(target);
        voided.EndTime = _timing.CurTime + ent.Comp.VoidedDuration;
        Dirty(target, voided);

        // Sunset: whoever's holding the victim (usually the Voidwalker itself, having dragged them
        // through a Glassify opening) needs to let go before the teleport, or the pull joint just
        // fights the new position on the next physics tick.
        if (TryComp<PullableComponent>(target, out var pullable) && pullable.Puller != null)
            _pulling.TryStopPull(target, pullable);

        // Sunset: send the victim back to the station instead of leaving them adrift in space
        // forever, and light up their suit sensors so the crew has a chance to notice/find them.
        if (TryFindRandomStationTile(out var coords))
        {
            _transform.SetCoordinates(target, coords);
            _transform.AttachToGridOrMap(target);
        }

        EnableSuitSensors(target);
        // Sunset: the tumor grows on its own, slower clock (TumorDuration) - it used to match the
        // 3-minute curse, which left almost no time to actually get surgery.
        ImplantVoidTumor(target, _timing.CurTime + ent.Comp.TumorDuration);

        _popup.PopupEntity(Loc.GetString("voidwalker-kidnap-success-self"), target, target, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("voidwalker-kidnap-success-voidwalker"), ent, ent, PopupType.Medium);
    }

    /// <summary>
    /// Grows a surgically-removable "void tumor" organ in the victim's torso cavity slot. Left in
    /// place, it slowly darkens and damages them until it finishes consuming them when the Voided
    /// curse would've expired anyway; cut out in time (existing "Extract Item" cavity surgery - no
    /// new steps needed, see OrganVoidTumor's comment) and it just stops.
    /// </summary>
    private void ImplantVoidTumor(EntityUid target, TimeSpan endTime)
    {
        var torso = _body.GetBodyChildrenOfType(target, BodyPartType.Torso).FirstOrDefault().Id;
        if (torso == default)
            return;

        var tumor = Spawn("OrganVoidTumor", Transform(target).Coordinates);
        if (!_body.InsertOrgan(torso, tumor, "cavity"))
        {
            QueueDel(tumor);
            return;
        }

        var tumorComp = EnsureComp<VoidTumorComponent>(tumor);
        tumorComp.Victim = target;
        tumorComp.StartTime = _timing.CurTime;
        tumorComp.EndTime = endTime;
        tumorComp.NextEffectTime = _timing.CurTime + TimeSpan.FromSeconds(20);
        Dirty(tumor, tumorComp);
    }

    private void OnTumorRemoved(Entity<VoidTumorComponent> ent, ref OrganRemovedFromBodyEvent args)
    {
        var victim = ent.Comp.Victim;

        // Cut out in time - the curse ends early along with the tumor.
        RemComp<VoidedComponent>(victim);
        RemComp<MutedComponent>(victim);
        RemComp<PacifiedComponent>(victim);

        _popup.PopupEntity(Loc.GetString("voidwalker-tumor-removed"), victim, victim, PopupType.LargeCaution);
        QueueDel(ent);
    }

    /// <summary>
    /// The tumor finished growing without being removed - the void keeps the victim, permanently.
    /// </summary>
    private void FinishTumor(EntityUid tumorUid, VoidTumorComponent tumor)
    {
        var victim = tumor.Victim;
        QueueDel(tumorUid);

        if (!Exists(victim))
            return;

        _damageable.TryChangeDamage(victim, new DamageSpecifier { DamageDict = { { "Cellular", 20f } } }, true);
        EnsureComp<VoidConsumedComponent>(victim);

        _popup.PopupEntity(Loc.GetString("voidwalker-tumor-consumed"), victim, victim, PopupType.LargeCaution);
    }

    private void EnableSuitSensors(EntityUid target)
    {
        var sensorQuery = EntityQueryEnumerator<SuitSensorComponent>();
        while (sensorQuery.MoveNext(out var sensorUid, out var sensor))
        {
            if (sensor.User != target)
                continue;

            _suitSensor.SetSensor((sensorUid, sensor), SuitSensorMode.SensorCords, target);
            break;
        }
    }

    /// <summary>
    /// Picks a random non-space, non-air-blocked tile on a random event-eligible station. Mirrors
    /// GameRuleSystem&lt;T&gt;.TryFindRandomTile, which isn't reusable here since this isn't a game rule.
    /// </summary>
    private bool TryFindRandomStationTile(out EntityCoordinates coords)
    {
        coords = EntityCoordinates.Invalid;

        var stations = new ValueList<EntityUid>();
        var stationQuery = AllEntityQuery<StationEventEligibleComponent>();
        while (stationQuery.MoveNext(out var stationUid, out _))
            stations.Add(stationUid);

        if (stations.Count == 0 || !TryComp<StationDataComponent>(stations[_random.Next(stations.Count)], out var stationData))
            return false;

        var weights = new Dictionary<Entity<MapGridComponent>, float>();
        foreach (var grid in stationData.Grids)
        {
            if (TryComp<MapGridComponent>(grid, out var gridComp))
                weights.Add((grid, gridComp), _map.GetAllTiles(grid, gridComp).Count());
        }

        if (weights.Count == 0)
            return false;

        var (targetGrid, targetGridComp) = _random.Pick(weights);
        var aabb = targetGridComp.LocalAABB;

        for (var i = 0; i < 10; i++)
        {
            var tile = new Vector2i(_random.Next((int) aabb.Left, (int) aabb.Right), _random.Next((int) aabb.Bottom, (int) aabb.Top));

            if (_atmosphere.IsTileSpace(targetGrid, Transform(targetGrid).MapUid, tile) ||
                _atmosphere.IsTileAirBlockedCached(targetGrid, tile))
                continue;

            coords = _map.GridTileToLocal(targetGrid, targetGridComp, tile);
            return true;
        }

        return false;
    }

    #endregion

    #region Glassify

    private void OnGlassify(Entity<VoidwalkerComponent> ent, ref VoidwalkerGlassifyActionEvent args)
    {
        if (args.Handled)
            return;

        var originCoords = Transform(ent).MapPosition;
        var targetCoords = _transform.ToMapCoordinates(args.Target);

        if (!_mapManager.TryFindGridAt(targetCoords, out var gridUid, out var gridComp))
        {
            _popup.PopupClient(Loc.GetString("voidwalker-glassify-invalid-target"), ent, ent);
            return;
        }

        // Grab every structure in the 3x3 area centered on the targeted tile at once - a window and
        // its supporting grille share a tile and both need to go transparent together, and this way
        // one cast opens a real opening instead of a single one-tile pinhole.
        var centerTile = _map.TileIndicesFor((gridUid, gridComp), targetCoords);
        var targets = new List<(EntityUid Uid, PhysicsComponent Physics)>();
        for (var dx = -1; dx <= 1; dx++)
        {
            for (var dy = -1; dy <= 1; dy++)
            {
                foreach (var candidate in _map.GetAnchoredEntities((gridUid, gridComp), centerTile + new Vector2i(dx, dy)))
                {
                    var isStructure = HasComp<AirtightComponent>(candidate) ||
                                       _tag.HasTag(candidate, "Window") ||
                                       _tag.HasTag(candidate, "Grille");

                    if (!isStructure ||
                        !TryComp<PhysicsComponent>(candidate, out var candidatePhysics) ||
                        !candidatePhysics.CanCollide ||
                        candidatePhysics.BodyType != BodyType.Static)
                        continue;

                    targets.Add((candidate, candidatePhysics));
                }
            }
        }

        if (targets.Count == 0)
        {
            _popup.PopupClient(Loc.GetString("voidwalker-glassify-invalid-target"), ent, ent);
            return;
        }

        // The wall/window/grille we're trying to glass is, itself, exactly the kind of obstruction
        // this check normally looks for - ignore the tile's own candidates or the raycast to them
        // always fails by hitting the very thing we're targeting.
        if (!_interaction.InRangeUnobstructed(originCoords, targetCoords, range: 4f,
                predicate: uid => targets.Any(t => t.Uid == uid)))
        {
            _popup.PopupClient(Loc.GetString("voidwalker-glassify-no-los"), ent, ent);
            return;
        }

        // Matches tg: a genuinely LIVE (powered) grille or window can't be phased - trying just zaps
        // you. Note ElectrifiedComponent.Enabled alone is NOT "currently live" - every grille has it
        // enabled by default whether or not any cable powers it - so we ask the electrocution system
        // to do the real power check, which shocks the Voidwalker if (and only if) it's actually hot.
        foreach (var (targetUid, _) in targets)
        {
            if (!_electrocution.TryDoElectrifiedAct(targetUid, ent))
                continue;

            args.Handled = true;
            _popup.PopupEntity(Loc.GetString("voidwalker-glassify-electrified"), ent, ent, PopupType.MediumCaution);
            return;
        }

        if (targets.All(t => HasComp<VoidGlassComponent>(t.Uid)))
        {
            _popup.PopupClient(Loc.GetString("voidwalker-glassify-already-glass"), ent, ent);
            return;
        }

        args.Handled = true;

        foreach (var (uid, physics) in targets)
        {
            var glass = EnsureComp<VoidGlassComponent>(uid);
            glass.EndTime = _timing.CurTime + ent.Comp.GlassifyDuration;
            Dirty(uid, glass);
            _physics.SetCanCollide(uid, false, body: physics);
        }

        _audio.PlayPvs(ent.Comp.GlassifySound, args.Target);
        _popup.PopupCoordinates(Loc.GetString("voidwalker-glassify-success"), args.Target, PopupType.Medium);
    }

    #endregion

    private void OnMobStateChanged(Entity<VoidwalkerComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        var coords = Transform(ent).Coordinates;
        Spawn(ent.Comp.DeathLoot, coords);
        _audio.PlayPvs(ent.Comp.DeathSound, coords);
        QueueDel(ent);
    }
}
