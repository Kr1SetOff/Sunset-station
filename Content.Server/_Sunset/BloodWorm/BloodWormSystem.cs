using System.Numerics;
using Content.Server.Popups;
using Content.Server.Weapons.Ranged.Systems;
using Content.Shared._Starlight.Medical.Body.Systems;
using Content.Shared._Sunset.BloodWorm;
using Content.Shared.Actions;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Content.Shared.Humanoid;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Rejuvenate;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.BloodWorm;

/// <summary>
/// 🌇Sunset🌇 - blood worm behavior, ported from tg/station's blood worm antagonist
/// (code/modules/antagonists/blood_worm): leeching blood for growth, maturing through cocoons,
/// corrosive blood spit, corpse puppeting (including nursing or reviving the host) and adult
/// reproduction. Numbers follow tg where they translate (500/1500 blood growth gates, three
/// stages, blood-burning host possession).
/// </summary>
public sealed class BloodWormSystem : EntitySystem
{
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private GunSystem _gun = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private MobStateSystem _mobState = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private SharedContainerSystem _containers = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedMindSystem _mind = default!;
    [Dependency] private SharedTransformSystem _xform = default!;

    private static readonly SoundPathSpecifier EnterBloodSound = new("/Audio/_Sunset/BloodWorm/enter_blood.ogg");
    private static readonly SoundPathSpecifier ExitBloodSound = new("/Audio/_Sunset/BloodWorm/exit_blood.ogg");

    private static readonly EntProtoId LeaveHostAction = "ActionBloodWormLeaveHost";
    private static readonly EntProtoId InjectAction = "ActionBloodWormInject";
    private static readonly EntProtoId ReviveHostAction = "ActionBloodWormReviveHost";

    private static readonly ProtoId<AlertPrototype> BloodAlert = "BloodWormBlood";

    // Real, mechanical objectives - all three are granted every time (see OnMindAdded).
    private static readonly EntProtoId[] RealObjectives =
    {
        "BloodWormKillObjective",
        "BloodWormConsumeObjective",
        "BloodWormSurviveObjective",
    };

    // Flavor-only roleplay objectives - exactly one is picked at random per worm.
    private static readonly EntProtoId[] RoleplayObjectives =
    {
        "BloodWormRpObjectiveSilent",
        "BloodWormRpObjectiveNest",
        "BloodWormRpObjectiveTaunt",
        "BloodWormRpObjectiveMimic",
        "BloodWormRpObjectiveSpare",
        "BloodWormRpObjectiveBloodline",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodWormComponent, MindAddedMessage>(OnMindAdded);

        SubscribeLocalEvent<BloodWormComponent, BloodWormLeechEvent>(OnLeech);
        SubscribeLocalEvent<BloodWormComponent, BloodWormLeechDoAfterEvent>(OnLeechDoAfter);
        SubscribeLocalEvent<BloodWormComponent, BloodWormMatureEvent>(OnMature);
        SubscribeLocalEvent<BloodWormComponent, BloodWormMatureDoAfterEvent>(OnMatureDoAfter);
        SubscribeLocalEvent<BloodWormComponent, BloodWormSpitEvent>(OnSpit);
        SubscribeLocalEvent<BloodWormComponent, BloodWormInvadeEvent>(OnInvade);
        SubscribeLocalEvent<BloodWormComponent, BloodWormInvadeDoAfterEvent>(OnInvadeDoAfter);
        SubscribeLocalEvent<BloodWormComponent, BloodWormReproduceEvent>(OnReproduce);

        SubscribeLocalEvent<BloodWormHostComponent, BloodWormLeaveHostEvent>(OnLeaveHost);
        SubscribeLocalEvent<BloodWormHostComponent, BloodWormInjectEvent>(OnInject);
        SubscribeLocalEvent<BloodWormHostComponent, BloodWormReviveHostEvent>(OnReviveHost);
        SubscribeLocalEvent<BloodWormHostComponent, BloodWormSpitEvent>(OnHostSpit);
        SubscribeLocalEvent<BloodWormHostComponent, MobStateChangedEvent>(OnHostMobStateChanged);
        SubscribeLocalEvent<BloodWormHostComponent, ComponentShutdown>(OnHostShutdown);

        SubscribeLocalEvent<BloodWormCocoonComponent, ComponentShutdown>(OnCocoonShutdown);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        // Puppeted hosts burn blood to keep moving; when it runs dry the worm gets squeezed out.
        var hostQuery = EntityQueryEnumerator<BloodWormHostComponent>();
        while (hostQuery.MoveNext(out var hostUid, out var host))
        {
            if (host.Worm is not { } worm || !TryComp<BloodWormComponent>(worm, out var wormComp))
                continue;

            _bloodstream.TryModifyBloodLevel(hostUid, FixedPoint2.New(-wormComp.HostBloodUpkeep * frameTime));

            if (_bloodstream.GetBloodLevel(hostUid) <= wormComp.HostEjectBloodLevel)
            {
                _popup.PopupEntity(Loc.GetString("blood-worm-host-out-of-blood"), hostUid, hostUid, PopupType.LargeCaution);
                EjectWorm(hostUid, host);
            }
        }

        // Cocoons hatch into the next stage once their timer is up.
        var cocoonQuery = EntityQueryEnumerator<BloodWormCocoonComponent>();
        while (cocoonQuery.MoveNext(out var cocoonUid, out var cocoon))
        {
            if (now >= cocoon.HatchAt)
                HatchCocoon(cocoonUid, cocoon);
        }
    }

    #region Objectives

    /// <summary>
    /// Grants objectives the moment a mind takes control of a blood worm, regardless of how it got
    /// here (ghost role raffle, egg hatching, admin spawn, game rule) - unlike the AntagSelection/
    /// AntagObjectives pipeline, this fires for every spawn path since it's just watching for the
    /// mind to attach to an entity that has BloodWormComponent.
    /// </summary>
    private void OnMindAdded(Entity<BloodWormComponent> ent, ref MindAddedMessage args)
    {
        if (!_mind.TryGetMind(ent.Owner, out var mindId, out var mind))
            return;

        foreach (var objective in RealObjectives)
            _mind.TryAddObjective(mindId, mind, objective);

        if (RoleplayObjectives.Length > 0)
            _mind.TryAddObjective(mindId, mind, _random.Pick(RoleplayObjectives));
    }

    #endregion

    #region Leech

    private void OnLeech(Entity<BloodWormComponent> ent, ref BloodWormLeechEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!HasComp<BloodstreamComponent>(target) || _bloodstream.GetBloodLevel(target) <= 0f)
        {
            _popup.PopupEntity(Loc.GetString("blood-worm-no-blood"), target, ent);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.LeechTime, new BloodWormLeechDoAfterEvent(), ent, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
            RequireCanInteract = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        _popup.PopupEntity(Loc.GetString("blood-worm-leech-start", ("worm", ent.Owner)), target, PopupType.MediumCaution);
        args.Handled = true;
    }

    private void OnLeechDoAfter(Entity<BloodWormComponent> ent, ref BloodWormLeechDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target is not { } target)
            return;

        if (!TryComp<BloodstreamComponent>(target, out var blood))
            return;

        var drained = ent.Comp.LeechAmount;
        if (!_bloodstream.TryModifyBloodLevel((target, blood), FixedPoint2.New(-drained)))
            return;

        ConsumeBlood(ent, drained);

        // Feeding also knits the worm's flesh back together.
        var heal = new DamageSpecifier();
        heal.DamageDict.Add("Slash", FixedPoint2.New(-drained * ent.Comp.LeechHealFraction));
        heal.DamageDict.Add("Blunt", FixedPoint2.New(-drained * ent.Comp.LeechHealFraction));
        _damageable.TryChangeDamage(ent.Owner, heal, ignoreResistances: true);

        _audio.PlayPvs(EnterBloodSound, target, AudioParams.Default.WithVolume(-4f));
        _popup.PopupEntity(Loc.GetString("blood-worm-leech-finish", ("worm", ent.Owner)), target, PopupType.MediumCaution);

        // Keep feeding as long as the victim holds still and has blood left.
        args.Repeat = _bloodstream.GetBloodLevel(target) > 0f;
        args.Handled = true;
    }

    private void ConsumeBlood(Entity<BloodWormComponent> ent, float amount)
    {
        ent.Comp.ConsumedBlood += amount;
        Dirty(ent);
        _alerts.ShowAlert(ent.Owner, BloodAlert);

        if (ent.Comp.MatureThreshold is { } threshold && ent.Comp.ConsumedBlood >= threshold)
            _popup.PopupEntity(Loc.GetString("blood-worm-ready-to-mature"), ent, ent);
    }

    #endregion

    #region Maturation and reproduction

    private void OnMature(Entity<BloodWormComponent> ent, ref BloodWormMatureEvent args)
    {
        if (args.Handled || ent.Comp.NextStage == null)
            return;

        if (ent.Comp.MatureThreshold is { } threshold && ent.Comp.ConsumedBlood < threshold)
        {
            _popup.PopupEntity(Loc.GetString("blood-worm-not-enough-blood",
                ("percent", (int) (ent.Comp.ConsumedBlood / threshold * 100))), ent, ent);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.MatureTime, new BloodWormMatureDoAfterEvent(), ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
            RequireCanInteract = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        _popup.PopupEntity(Loc.GetString("blood-worm-maturing", ("worm", ent.Owner)), ent, PopupType.MediumCaution);
        args.Handled = true;
    }

    /// <summary>
    /// The worm finishes curling up: it vanishes into the cocoon (container-hidden, same technique
    /// as host puppeting) instead of the cocoon being a disconnected decoration. See HatchCocoon for
    /// what happens when the timer is up.
    /// </summary>
    private void OnMatureDoAfter(Entity<BloodWormComponent> ent, ref BloodWormMatureDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.NextStage is not { } nextStage || ent.Comp.CocoonPrototype is not { } cocoonProto)
            return;

        var cocoonUid = Spawn(cocoonProto, Transform(ent).Coordinates);
        var cocoon = EnsureComp<BloodWormCocoonComponent>(cocoonUid);
        cocoon.Worm = ent.Owner;
        cocoon.NextStage = nextStage;
        cocoon.HatchAt = _timing.CurTime + ent.Comp.CocoonDuration;

        var container = _containers.EnsureContainer<ContainerSlot>(cocoonUid, BloodWormCocoonComponent.ContainerId);
        _containers.Insert(ent.Owner, container);

        _audio.PlayPvs(EnterBloodSound, cocoonUid);
        _adminLogger.Add(LogType.Action, LogImpact.Low,
            $"Blood worm {ToPrettyString(ent.Owner)} cocooned at {ToPrettyString(cocoonUid)}, will emerge as {nextStage}");

        args.Handled = true;
    }

    /// <summary>
    /// A cocoon's timer is up: delete it and its contained worm, and spawn the next stage in its
    /// place, carrying the mind and lifetime consumed-blood total across.
    /// </summary>
    private void HatchCocoon(EntityUid cocoonUid, BloodWormCocoonComponent cocoon)
    {
        var coords = Transform(cocoonUid).Coordinates;
        var oldWorm = cocoon.Worm;

        var newWorm = Spawn(cocoon.NextStage, coords);

        if (TryComp<BloodWormComponent>(newWorm, out var newComp) && TryComp<BloodWormComponent>(oldWorm, out var oldComp))
        {
            newComp.ConsumedBlood = oldComp.ConsumedBlood;
            Dirty(newWorm, newComp);
        }

        if (_mind.TryGetMind(oldWorm, out var mindId, out var mind))
            _mind.TransferTo(mindId, newWorm, mind: mind);

        _audio.PlayPvs(ExitBloodSound, newWorm);
        _popup.PopupEntity(Loc.GetString("blood-worm-cocoon-hatch"), newWorm, PopupType.MediumCaution);
        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"Blood worm cocoon {ToPrettyString(cocoonUid)} hatched into {ToPrettyString(newWorm)}");

        QueueDel(oldWorm);
        QueueDel(cocoonUid);
    }

    /// <summary>
    /// The cocoon got destroyed (crew smashed it) before it could hatch - the worm inside dies with
    /// it. Guarded against firing during the normal HatchCocoon deletion (worm is already queued).
    /// </summary>
    private void OnCocoonShutdown(Entity<BloodWormCocoonComponent> ent, ref ComponentShutdown args)
    {
        if (!TerminatingOrDeleted(ent.Comp.Worm))
            QueueDel(ent.Comp.Worm);
    }

    private void OnReproduce(Entity<BloodWormComponent> ent, ref BloodWormReproduceEvent args)
    {
        if (args.Handled || ent.Comp.EggPrototype is not { } egg)
            return;

        if (ent.Comp.ConsumedBlood < ent.Comp.ReproduceCost)
        {
            _popup.PopupEntity(Loc.GetString("blood-worm-not-enough-blood",
                ("percent", (int) (ent.Comp.ConsumedBlood / ent.Comp.ReproduceCost * 100))), ent, ent);
            return;
        }

        ent.Comp.ConsumedBlood -= ent.Comp.ReproduceCost;
        Dirty(ent);
        _alerts.ShowAlert(ent.Owner, BloodAlert);

        var spawned = Spawn(egg, Transform(ent).Coordinates);
        _audio.PlayPvs(ExitBloodSound, spawned);
        _popup.PopupEntity(Loc.GetString("blood-worm-reproduce", ("worm", ent.Owner)), ent, PopupType.MediumCaution);

        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"Blood worm {ToPrettyString(ent.Owner)} laid an egg cocoon {ToPrettyString(spawned)}");

        args.Handled = true;
    }

    #endregion

    #region Spit

    private void OnSpit(Entity<BloodWormComponent> ent, ref BloodWormSpitEvent args)
    {
        if (args.Handled || ent.Comp.SpitProjectile is not { } projectile)
            return;

        if (DoSpit(ent.Owner, projectile, args.Target, ent.Comp.SpitHealthCost))
            args.Handled = true;
    }

    /// <summary>
    /// The host body spits using the worm's stats - tg lets possessed hosts spit too.
    /// </summary>
    private void OnHostSpit(Entity<BloodWormHostComponent> ent, ref BloodWormSpitEvent args)
    {
        if (args.Handled || ent.Comp.Worm is not { } worm || !TryComp<BloodWormComponent>(worm, out var wormComp))
            return;

        if (wormComp.SpitProjectile is not { } projectile)
            return;

        if (DoSpit(ent.Owner, projectile, args.Target, wormComp.SpitHealthCost))
            args.Handled = true;
    }

    private bool DoSpit(EntityUid shooter, EntProtoId projectilePrototype, EntityCoordinates target, float healthCost)
    {
        var from = _xform.GetMapCoordinates(shooter);
        var to = _xform.ToMapCoordinates(target);

        if (from.MapId != to.MapId)
            return false;

        var direction = to.Position - from.Position;
        if (direction == Vector2.Zero)
            return false;

        var projectile = Spawn(projectilePrototype, from);
        _gun.ShootProjectile(projectile, direction, Vector2.Zero, shooter, shooter);

        // Spitting corrosive blood costs your own substance.
        var selfDamage = new DamageSpecifier();
        selfDamage.DamageDict.Add("Slash", FixedPoint2.New(healthCost));
        _damageable.TryChangeDamage(shooter, selfDamage, ignoreResistances: true);

        _audio.PlayPvs(ExitBloodSound, shooter, AudioParams.Default.WithVolume(-6f));
        return true;
    }

    #endregion

    #region Corpse puppeting

    private void OnInvade(Entity<BloodWormComponent> ent, ref BloodWormInvadeEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        if (!HasComp<HumanoidAppearanceComponent>(target) || !_mobState.IsDead(target))
        {
            _popup.PopupEntity(Loc.GetString("blood-worm-invade-not-corpse"), target, ent);
            return;
        }

        if (HasComp<BloodWormHostComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("blood-worm-invade-occupied"), target, ent);
            return;
        }

        if (_bloodstream.GetBloodLevel(target) < ent.Comp.InvadeMinBloodLevel)
        {
            _popup.PopupEntity(Loc.GetString("blood-worm-no-blood"), target, ent);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.InvadeTime, new BloodWormInvadeDoAfterEvent(), ent, target)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
            RequireCanInteract = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        _popup.PopupEntity(Loc.GetString("blood-worm-invade-start", ("worm", ent.Owner)), target, PopupType.LargeCaution);
        args.Handled = true;
    }

    private void OnInvadeDoAfter(Entity<BloodWormComponent> ent, ref BloodWormInvadeDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target is not { } body)
            return;

        if (HasComp<BloodWormHostComponent>(body) || !_mobState.IsDead(body))
            return;

        var host = EnsureComp<BloodWormHostComponent>(body);
        host.Worm = ent.Owner;
        ent.Comp.Host = body;

        // The worm burrows into the corpse and jump-starts it: the body walks again, but it's the
        // worm at the wheel, burning the corpse's blood as fuel (see Update).
        var container = _containers.EnsureContainer<ContainerSlot>(body, BloodWormHostComponent.ContainerId);
        _containers.Insert(ent.Owner, container);

        RaiseLocalEvent(body, new RejuvenateEvent());

        if (_mind.TryGetMind(ent.Owner, out var mindId, out var mind))
            _mind.TransferTo(mindId, body, mind: mind);

        host.LeaveAction = _actions.AddAction(body, LeaveHostAction);
        host.InjectAction = _actions.AddAction(body, InjectAction);
        host.ReviveAction = _actions.AddAction(body, ReviveHostAction);

        _audio.PlayPvs(EnterBloodSound, body);
        _popup.PopupEntity(Loc.GetString("blood-worm-invade-finish"), body, PopupType.LargeCaution);
        _adminLogger.Add(LogType.Action, LogImpact.High,
            $"Blood worm {ToPrettyString(ent.Owner)} invaded and puppeted the corpse of {ToPrettyString(body)}");

        args.Handled = true;
    }

    private void OnLeaveHost(Entity<BloodWormHostComponent> ent, ref BloodWormLeaveHostEvent args)
    {
        if (args.Handled)
            return;

        EjectWorm(ent.Owner, ent.Comp);
        args.Handled = true;
    }

    /// <summary>
    /// tg's Inject Blood - heals the host's wounds/blood at the cost of the worm's own health. Only
    /// usable while the host is alive (a dead host needs Revive Host instead).
    /// </summary>
    private void OnInject(Entity<BloodWormHostComponent> ent, ref BloodWormInjectEvent args)
    {
        if (args.Handled || ent.Comp.Worm is not { } worm || !TryComp<BloodWormComponent>(worm, out var wormComp))
            return;

        if (_mobState.IsDead(ent.Owner))
            return;

        if (!TryComp<DamageableComponent>(worm, out var wormDamage) ||
            wormDamage.TotalDamage + wormComp.InjectHealthCost >= 100)
        {
            _popup.PopupEntity(Loc.GetString("blood-worm-inject-not-enough-health"), ent, ent);
            return;
        }

        var selfDamage = new DamageSpecifier();
        selfDamage.DamageDict.Add("Slash", FixedPoint2.New(wormComp.InjectHealthCost));
        _damageable.TryChangeDamage(worm, selfDamage, ignoreResistances: true);

        _bloodstream.TryModifyBloodLevel(ent.Owner, FixedPoint2.New(wormComp.InjectBloodRestore));

        var heal = new DamageSpecifier();
        heal.DamageDict.Add("Blunt", FixedPoint2.New(-wormComp.InjectDamageHealed));
        heal.DamageDict.Add("Burn", FixedPoint2.New(-wormComp.InjectDamageHealed));
        _damageable.TryChangeDamage(ent.Owner, heal, ignoreResistances: true);

        _audio.PlayPvs(EnterBloodSound, ent.Owner, AudioParams.Default.WithVolume(-4f));
        _popup.PopupEntity(Loc.GetString("blood-worm-inject-success"), ent, ent, PopupType.Medium);

        args.Handled = true;
    }

    /// <summary>
    /// tg's Revive Host - restarts a dead host's blood circulation. Only usable while the worm is
    /// still inside a dead-but-not-ejected host (see OnHostMobStateChanged - death no longer
    /// auto-ejects the worm, giving this a real window to matter).
    /// </summary>
    private void OnReviveHost(Entity<BloodWormHostComponent> ent, ref BloodWormReviveHostEvent args)
    {
        if (args.Handled || ent.Comp.Worm is not { } worm || !TryComp<BloodWormComponent>(worm, out var wormComp))
            return;

        if (!_mobState.IsDead(ent.Owner))
        {
            _popup.PopupEntity(Loc.GetString("blood-worm-revive-not-dead"), ent, ent);
            return;
        }

        if (!TryComp<DamageableComponent>(worm, out var wormDamage) ||
            wormDamage.TotalDamage + wormComp.ReviveHealthCost >= 100)
        {
            _popup.PopupEntity(Loc.GetString("blood-worm-revive-not-enough-health"), ent, ent);
            return;
        }

        var selfDamage = new DamageSpecifier();
        selfDamage.DamageDict.Add("Slash", FixedPoint2.New(wormComp.ReviveHealthCost));
        _damageable.TryChangeDamage(worm, selfDamage, ignoreResistances: true);

        _bloodstream.TryModifyBloodLevel(ent.Owner, FixedPoint2.New(wormComp.ReviveBloodRestore));
        _mobState.ChangeMobState(ent.Owner, MobState.Alive);

        _audio.PlayPvs(EnterBloodSound, ent.Owner);
        _popup.PopupEntity(Loc.GetString("blood-worm-revive-success"), ent.Owner, PopupType.LargeCaution);
        _adminLogger.Add(LogType.Action, LogImpact.Medium,
            $"Blood worm {ToPrettyString(worm)} revived its host {ToPrettyString(ent.Owner)}");

        args.Handled = true;
    }

    private void OnHostMobStateChanged(Entity<BloodWormHostComponent> ent, ref MobStateChangedEvent args)
    {
        // Combat death no longer squeezes the worm out immediately - it gets a chance to use
        // Revive Host (or bail out manually with Leave Host) instead. Running out of host blood
        // (see Update) still ejects unconditionally, same as tg.
        if (args.NewMobState == MobState.Dead)
            _popup.PopupEntity(Loc.GetString("blood-worm-host-died"), ent, ent, PopupType.LargeCaution);
    }

    private void OnHostShutdown(Entity<BloodWormHostComponent> ent, ref ComponentShutdown args)
    {
        // Safety net for the body getting deleted/gibbed with the worm still inside.
        if (ent.Comp.Worm is { } worm && !TerminatingOrDeleted(worm) && _containers.IsEntityInContainer(worm))
            EjectWorm(ent.Owner, ent.Comp, silent: true);
    }

    private void EjectWorm(EntityUid body, BloodWormHostComponent host, bool silent = false)
    {
        if (host.Worm is not { } worm)
            return;

        host.Worm = null;

        if (host.LeaveAction is { } leaveAction)
        {
            _actions.RemoveAction(body, leaveAction);
            host.LeaveAction = null;
        }

        if (host.InjectAction is { } injectAction)
        {
            _actions.RemoveAction(body, injectAction);
            host.InjectAction = null;
        }

        if (host.ReviveAction is { } reviveAction)
        {
            _actions.RemoveAction(body, reviveAction);
            host.ReviveAction = null;
        }

        if (!TerminatingOrDeleted(worm))
        {
            _containers.TryRemoveFromContainer(worm);
            _xform.SetCoordinates(worm, Transform(body).Coordinates);

            if (_mind.TryGetMind(body, out var mindId, out var mind))
                _mind.TransferTo(mindId, worm, mind: mind);

            if (TryComp<BloodWormComponent>(worm, out var wormComp))
                wormComp.Host = null;
        }

        RemCompDeferred<BloodWormHostComponent>(body);

        if (TerminatingOrDeleted(body))
            return;

        // The body drops dead again the moment its parasite pilot bails out.
        if (!_mobState.IsDead(body))
            _mobState.ChangeMobState(body, MobState.Dead);

        if (!silent)
        {
            _audio.PlayPvs(ExitBloodSound, body);
            _popup.PopupEntity(Loc.GetString("blood-worm-leave-host"), body, PopupType.LargeCaution);
        }
    }

    #endregion
}
