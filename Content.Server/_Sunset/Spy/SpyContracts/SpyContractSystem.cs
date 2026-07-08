using System.Linq;
using Content.Server.Emp;
using Content.Server.Pinpointer;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared._Sunset.Spy.SpyContracts;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Events;
using Content.Shared.Pinpointer;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Roles.Jobs;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Content.Shared.Whitelist;
using Robust.Server.GameObjects;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.Spy.SpyContracts;

/// <summary>
/// Drives the Spy's contract board (Bounty System), ported from frostek3122-byte/sunset-station
/// and adapted to this fork's APIs, plus additions the source didn't have: a rotating board
/// (SpyContractsComponent.Board refreshes every RotationInterval instead of "roll one contract
/// on demand"), reputation shown directly in the custom uplink BUI, and physically planted bugs
/// (see PlantTracker) instead of a held-item DoAfter.
///
/// Five contract families, all driven by planting the Spy Tracker gadget on the target:
///  - Surveillance / SurveillanceProximity: plant the bug and walk away - it counts down Duration
///    on its own (UpdatePlantedTrackers), no need to stay nearby. Anyone who finds and picks it up
///    before then blows the surveillance (OnPlantedTrackerPickedUp fails the contract).
///  - Sabotage: same planting/timer, then applies SabotageEffect on completion.
///  - CollectData: same planting/timer, no damage to the target.
///  - Assassinate: no tracker/timer at all - completes when the target dies with the spy as killer.
///
/// Reused wholesale from existing SS14 systems: StoreSystem (reward payout via the uplink's
/// currency balance), PinpointerSystem (aims the spy pinpointer at the target),
/// EntityWhitelistSystem (target filtering), BatterySystem/SharedDoorSystem/EmpSystem/
/// DamageableSystem (sabotage effects), SharedJobSystem/SharedMindSystem (job-filtered targets).
/// </summary>
public sealed class SpyContractSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PinpointerSystem _pinpointer = default!;
    [Dependency] private readonly StoreSystem _store = default!;
    [Dependency] private readonly SharedBatterySystem _battery = default!;
    [Dependency] private readonly SharedDoorSystem _door = default!;
    [Dependency] private readonly EmpSystem _emp = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly StationSystem _station = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    /// <summary>Default reward per difficulty, used when a contract doesn't set an explicit Reward.</summary>
    private static readonly Dictionary<SpyContractDifficulty, int> DifficultyReward = new()
    {
        [SpyContractDifficulty.Easy] = 25,
        [SpyContractDifficulty.Medium] = 50,
        [SpyContractDifficulty.Hard] = 100,
    };

    /// <summary>Reputation delta (gain on completion, loss on abandon) per difficulty.</summary>
    private static readonly Dictionary<SpyContractDifficulty, int> DifficultyReputation = new()
    {
        [SpyContractDifficulty.Easy] = 1,
        [SpyContractDifficulty.Medium] = 2,
        [SpyContractDifficulty.Hard] = 3,
    };

    /// <summary>How much one reputation point changes payout (0.1 = +10% per point).</summary>
    private const float ReputationPayFactor = 0.1f;

    /// <summary>Pay never drops below this fraction of the base reward, even at very negative reputation.</summary>
    private const float MinPayMultiplier = 0.25f;

    private const float EmpEnergyConsumption = 100000f;
    private static readonly TimeSpan EmpDuration = TimeSpan.FromSeconds(30);
    private const float BreakDamage = 100000f;

    /// <summary>How often (seconds) to recheck/refresh a planted tracker's progress.</summary>
    private const float ProximityCheckInterval = 1f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SpyContractsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SpyContractsComponent, BoundUIOpenedEvent>(OnUiOpened);
        SubscribeLocalEvent<SpyContractsComponent, StoreRequestUpdateInterfaceMessage>(OnRequestUpdate);
        SubscribeLocalEvent<SpyContractsComponent, SpyAcceptContractMessage>(OnAcceptContract);
        SubscribeLocalEvent<SpyContractsComponent, SpyAbandonContractMessage>(OnAbandonContract);

        SubscribeLocalEvent<SpyTrackerComponent, AfterInteractEvent>(OnTrackerAfterInteract);
        SubscribeLocalEvent<SpyTrackerComponent, PullAttemptEvent>(OnTrackerPullAttempt);
        SubscribeLocalEvent<SpyPlantedTrackerComponent, GotEquippedHandEvent>(OnPlantedTrackerPickedUp);

        SubscribeLocalEvent<MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMapInit(Entity<SpyContractsComponent> ent, ref MapInitEvent args)
    {
        RollBoard(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<SpyContractsComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now >= comp.NextRotation)
                RollBoard((uid, comp));
        }

        UpdatePlantedTrackers(frameTime, now);
    }

    #region Board / accept / abandon

    private void RollBoard(Entity<SpyContractsComponent> ent)
    {
        ent.Comp.Board = _proto.EnumeratePrototypes<SpyContractPrototype>()
            .Select(c => c.ID)
            .OrderBy(_ => _random.Next())
            .Take(ent.Comp.BoardSize)
            .Select(id => new ProtoId<SpyContractPrototype>(id))
            .ToList();
        ent.Comp.NextRotation = _timing.CurTime + ent.Comp.RotationInterval;

        RefreshUi(ent, null);
    }

    private void OnUiOpened(Entity<SpyContractsComponent> ent, ref BoundUIOpenedEvent args)
    {
        if (args.UiKey is StoreUiKey.Key)
            RefreshUi(ent, args.Actor);
    }

    private void OnRequestUpdate(Entity<SpyContractsComponent> ent, ref StoreRequestUpdateInterfaceMessage args)
    {
        RefreshUi(ent, args.Actor);
    }

    private void OnAcceptContract(Entity<SpyContractsComponent> ent, ref SpyAcceptContractMessage args)
    {
        var comp = ent.Comp;

        if (comp.ActiveContract != null)
        {
            _popup.PopupEntity(Loc.GetString("spy-contract-already-active"), args.Actor, args.Actor, PopupType.MediumCaution);
            return;
        }

        if (!comp.Board.Contains(args.ContractId) || !_proto.TryIndex(args.ContractId, out var contract))
            return;

        var target = PickTarget(args.Actor, contract);
        if (target == null)
        {
            _popup.PopupEntity(Loc.GetString("spy-contract-no-target"), args.Actor, args.Actor, PopupType.MediumCaution);
            return;
        }

        AssignContract(ent, args.Actor, contract, target.Value);
    }

    private void OnAbandonContract(Entity<SpyContractsComponent> ent, ref SpyAbandonContractMessage args)
    {
        var comp = ent.Comp;

        if (comp.ActiveContract == null)
        {
            _popup.PopupEntity(Loc.GetString("spy-contract-nothing-to-abandon"), args.Actor, args.Actor);
            return;
        }

        if (_proto.TryIndex(comp.ActiveContract.Value, out var contract))
            comp.Reputation -= GetReputationWeight(contract.Difficulty);

        ClearActiveContract(ent, args.Actor);

        _popup.PopupEntity(
            Loc.GetString("spy-contract-abandoned", ("reputation", comp.Reputation)),
            args.Actor, args.Actor, PopupType.Medium);

        RefreshUi(ent, args.Actor);
    }

    private void AssignContract(Entity<SpyContractsComponent> ent, EntityUid spy, SpyContractPrototype contract, EntityUid target)
    {
        var comp = ent.Comp;
        comp.ActiveContract = contract.ID;
        comp.ActiveTarget = target;
        comp.ActiveReward = GetReward(contract, comp.Reputation);
        comp.SurveillanceActive = false;
        comp.Accumulated = 0f;
        comp.CheckAccumulator = 0f;

        PointPinpointer(spy, target);

        _popup.PopupEntity(
            Loc.GetString("spy-contract-accepted", ("name", Loc.GetString(contract.Name)), ("reputation", comp.Reputation)),
            spy, spy, PopupType.Medium);

        RefreshUi(ent, spy);
    }

    private void ClearActiveContract(Entity<SpyContractsComponent> ent, EntityUid spy)
    {
        var comp = ent.Comp;
        comp.ActiveContract = null;
        comp.ActiveTarget = null;
        comp.ActiveReward = 0;
        comp.SurveillanceActive = false;
        comp.Accumulated = 0f;
        comp.CheckAccumulator = 0f;

        ClearPinpointer(spy);
    }

    #endregion

    #region Tracker interaction / planting

    /// <summary>
    /// The tracker can't be pulled/dragged at all - neither loose on the floor nor while planted -
    /// so a found bug has to be actually picked up (which is what blows the contract), not towed
    /// around the station.
    /// </summary>
    private void OnTrackerPullAttempt(EntityUid uid, SpyTrackerComponent component, PullAttemptEvent args)
    {
        args.Cancelled = true;
    }

    private void OnTrackerAfterInteract(Entity<SpyTrackerComponent> tracker, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target == null || HasComp<SpyPlantedTrackerComponent>(tracker.Owner))
            return;

        if (!TryFindUplink(args.User, out var uplinkEnt) || !TryComp<SpyContractsComponent>(uplinkEnt, out var state) || state.ActiveContract == null)
        {
            _popup.PopupEntity(Loc.GetString("spy-tracker-no-contract"), args.User, args.User);
            return;
        }

        if (!_proto.TryIndex(state.ActiveContract.Value, out var contract))
            return;

        if (contract.Type == SpyContractType.Assassinate)
        {
            _popup.PopupEntity(Loc.GetString("spy-tracker-assassinate-hint"), args.User, args.User, PopupType.MediumCaution);
            return;
        }

        var valid = state.ActiveTarget == args.Target || IsValidTarget(contract, args.Target.Value);
        if (!valid)
        {
            _popup.PopupEntity(Loc.GetString("spy-tracker-wrong-target"), args.User, args.User, PopupType.MediumCaution);
            return;
        }

        PlantTracker(uplinkEnt.Value, state, args.User, tracker.Owner, args.Target.Value, contract);
        args.Handled = true;
    }

    /// <summary>
    /// Plants the bug on the target: the same SpyTracker item leaves the spy's hands and sits
    /// visibly where it was placed (no separate "planted" entity - it's still the same tracker,
    /// same name, same everything), and counts down Duration on its own from here (see
    /// UpdatePlantedTrackers) - no more holding still. Anyone (not just the spy) can just walk up
    /// and pick it up like any other item; see OnPlantedTrackerPickedUp for what that does before
    /// vs. after it finishes. Once picked up (or once it naturally finishes), SpyPlantedTracker
    /// comes off and it's a completely ordinary, re-plantable Spy Tracker again.
    /// </summary>
    private void PlantTracker(EntityUid uplinkEnt, SpyContractsComponent state, EntityUid spy, EntityUid trackerItem, EntityUid target, SpyContractPrototype contract)
    {
        state.ActiveTarget = target;
        state.SurveillanceActive = true;
        state.Accumulated = 0f;
        state.CheckAccumulator = 0f;

        var coords = Transform(target).Coordinates;
        _hands.TryDrop(spy, trackerItem, coords);

        var plantedComp = EnsureComp<SpyPlantedTrackerComponent>(trackerItem);
        plantedComp.Spy = spy;
        plantedComp.Uplink = uplinkEnt;
        plantedComp.ContractId = contract.ID;
        plantedComp.Target = target;
        plantedComp.EndTime = _timing.CurTime + TimeSpan.FromSeconds(contract.Duration);

        _popup.PopupEntity(Loc.GetString("spy-tracker-planted"), spy, spy);
        RefreshUi((uplinkEnt, state), spy);
    }

    /// <summary>
    /// A planted bug is a completely ordinary item to pick up - anyone can. If that happens while
    /// SpyPlantedTrackerComponent is still present, the contract hasn't finished yet, so this counts
    /// as the bug being found: the surveillance is blown and the spy's contract fails outright
    /// rather than completing. Either way, removing the component here (whether found early or
    /// picked up after CompleteContract already removed it via UpdatePlantedTrackers) is what turns
    /// it back into a normal, re-plantable Spy Tracker.
    /// </summary>
    private void OnPlantedTrackerPickedUp(Entity<SpyPlantedTrackerComponent> ent, ref GotEquippedHandEvent args)
    {
        var comp = ent.Comp;
        RemComp<SpyPlantedTrackerComponent>(ent.Owner);

        if (!TryComp<SpyContractsComponent>(comp.Uplink, out var state) || state.ActiveContract != comp.ContractId)
            return;

        ClearActiveContract((comp.Uplink, state), comp.Spy);
        _popup.PopupEntity(Loc.GetString("spy-contract-bug-found"), comp.Spy, comp.Spy, PopupType.MediumCaution);
        RefreshUi((comp.Uplink, state), comp.Spy);

        if (args.User != comp.Spy)
            _popup.PopupEntity(Loc.GetString("spy-tracker-found"), args.User, args.User);
    }

    private void OnMobStateChanged(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || args.Origin is not { } killer)
            return;

        if (!TryFindUplink(killer, out var uplinkEnt) || !TryComp<SpyContractsComponent>(uplinkEnt, out var state)
            || state.ActiveContract == null || state.ActiveTarget != args.Target)
            return;

        if (!_proto.TryIndex(state.ActiveContract.Value, out var contract) || contract.Type != SpyContractType.Assassinate)
            return;

        CompleteContract((uplinkEnt.Value, state), killer, contract, args.Target);
    }

    #endregion

    #region Planted tracker update

    /// <summary>
    /// Ticks down every planted-but-not-yet-discovered bug. Unlike the old proximity accumulation,
    /// this doesn't care where the spy is - the bug does the work on its own once it's placed.
    /// </summary>
    private void UpdatePlantedTrackers(float frameTime, TimeSpan now)
    {
        var query = EntityQueryEnumerator<SpyPlantedTrackerComponent>();
        while (query.MoveNext(out var uid, out var planted))
        {
            if (!TryComp<SpyContractsComponent>(planted.Uplink, out var state)
                || state.ActiveContract != planted.ContractId
                || !_proto.TryIndex(planted.ContractId, out var contract))
            {
                // The contract this bug belonged to isn't active anymore for some other reason
                // (abandoned, uplink destroyed) - it's just an inert item from here on.
                RemComp<SpyPlantedTrackerComponent>(uid);
                continue;
            }

            if (now >= planted.EndTime)
            {
                state.Accumulated = contract.Duration;
                CompleteContract((planted.Uplink, state), planted.Spy, contract, planted.Target);
                RemComp<SpyPlantedTrackerComponent>(uid);
                continue;
            }

            planted.CheckAccumulator += frameTime;
            if (planted.CheckAccumulator < ProximityCheckInterval)
                continue;
            planted.CheckAccumulator = 0f;

            state.Accumulated = MathF.Max(0f, contract.Duration - (float) (planted.EndTime - now).TotalSeconds);
            RefreshUi((planted.Uplink, state), planted.Spy);
        }
    }

    #endregion

    #region Completion / sabotage

    private void CompleteContract(Entity<SpyContractsComponent> ent, EntityUid spy, SpyContractPrototype contract, EntityUid? target)
    {
        var comp = ent.Comp;

        ApplySabotage(contract, target);

        var currency = new Dictionary<string, FixedPoint2> { ["SpyCredit"] = comp.ActiveReward };
        _store.TryAddCurrency(currency, ent.Owner);

        // Take it off the board so it can't be immediately re-accepted - it can only come back
        // (if it comes back at all) on the next RollBoard.
        comp.Board.Remove(contract.ID);

        comp.Completed += 1;
        comp.Reputation += GetReputationWeight(contract.Difficulty);

        var reward = comp.ActiveReward;
        var reputation = comp.Reputation;
        ClearActiveContract(ent, spy);

        _popup.PopupEntity(
            Loc.GetString("spy-contract-completed", ("reward", reward), ("reputation", reputation)),
            spy, spy, PopupType.Large);

        RefreshUi(ent, spy);
    }

    private void ApplySabotage(SpyContractPrototype contract, EntityUid? target)
    {
        if (target is not { } uid)
            return;

        switch (contract.SabotageEffect)
        {
            case SpySabotageEffect.DrainBattery:
                if (HasComp<BatteryComponent>(uid))
                    _battery.SetCharge(uid, 0f);
                break;

            case SpySabotageEffect.BoltDoor:
                if (TryComp<DoorBoltComponent>(uid, out var bolt))
                    _door.SetBoltsDown((uid, bolt), true);
                break;

            case SpySabotageEffect.EmpPulse:
                _emp.DoEmpEffects(uid, EmpEnergyConsumption, EmpDuration);
                break;

            case SpySabotageEffect.BreakDevice:
                if (_proto.TryIndex<DamageTypePrototype>("Structural", out var structural))
                    _damageable.TryChangeDamage(uid, new DamageSpecifier(structural, FixedPoint2.New(BreakDamage)), ignoreResistances: true);
                break;
        }
    }

    #endregion

    #region Rewards / reputation

    private int GetReward(SpyContractPrototype contract, int reputation)
    {
        var baseReward = contract.Reward > 0 ? contract.Reward : DifficultyReward.GetValueOrDefault(contract.Difficulty);
        if (baseReward <= 0)
            return 0;

        var multiplier = MathF.Max(MinPayMultiplier, 1f + reputation * ReputationPayFactor);
        return Math.Max(1, (int) MathF.Round(baseReward * multiplier));
    }

    private int GetReputationWeight(SpyContractDifficulty difficulty) => DifficultyReputation.GetValueOrDefault(difficulty, 1);

    #endregion

    #region Pinpointer

    private void PointPinpointer(EntityUid spy, EntityUid target)
    {
        if (!TryFindItem<SpyPinpointerComponent>(spy, out var pin) || !TryComp<PinpointerComponent>(pin, out var pinComp))
            return;

        if (!pinComp.IsActive)
            _pinpointer.TogglePinpointer(pin.Value, pinComp);
        _pinpointer.SetTarget(pin.Value, target, pinComp);
    }

    private void ClearPinpointer(EntityUid spy)
    {
        if (TryFindItem<SpyPinpointerComponent>(spy, out var pin) && TryComp<PinpointerComponent>(pin, out var pinComp) && pinComp.IsActive)
            _pinpointer.TogglePinpointer(pin.Value, pinComp);
    }

    #endregion

    #region Target validation / selection

    private bool IsValidTarget(SpyContractPrototype contract, EntityUid target)
    {
        if (contract.TargetWhitelist == null || !_whitelist.IsValid(contract.TargetWhitelist, target))
            return false;

        if (contract.Type == SpyContractType.Assassinate && !_mobState.IsAlive(target))
            return false;

        if (contract.TargetJobs.Count > 0)
        {
            if (!_mind.TryGetMind(target, out var mindId, out _))
                return false;
            if (!_jobs.MindTryGetJobId(mindId, out var job) || job == null)
                return false;
            if (!contract.TargetJobs.Contains(job.Value))
                return false;
        }

        return true;
    }

    private EntityUid? PickTarget(EntityUid spy, SpyContractPrototype contract)
    {
        if (contract.TargetWhitelist == null)
            return null;

        var spyMap = Transform(spy).MapID;
        var candidates = new List<EntityUid>();

        var query = AllEntityQuery<TransformComponent>();
        while (query.MoveNext(out var uid, out var xform))
        {
            if (uid == spy || xform.MapID != spyMap || !IsValidTarget(contract, uid) || !IsOnStation(uid))
                continue;

            candidates.Add(uid);
        }

        return candidates.Count == 0 ? null : _random.Pick(candidates);
    }

    private bool IsOnStation(EntityUid uid) => _station.GetOwningStation(uid) != null;

    #endregion

    #region Item lookup

    /// <summary>Finds the spy's SpyContracts uplink item and returns it (not just a marker) so we
    /// can read/write board+reputation state without a second recursive search.</summary>
    private bool TryFindUplink(EntityUid spy, out EntityUid? uplink) => TryFindItem<SpyContractsComponent>(spy, out uplink);

    private bool TryFindItem<T>(EntityUid root, out EntityUid? found) where T : IComponent
    {
        found = null;
        var stack = new Stack<EntityUid>();
        stack.Push(root);

        while (stack.Count > 0)
        {
            var ent = stack.Pop();

            if (!TryComp<ContainerManagerComponent>(ent, out var manager))
                continue;

            foreach (var container in manager.Containers.Values)
            {
                foreach (var contained in container.ContainedEntities)
                {
                    if (HasComp<T>(contained))
                    {
                        found = contained;
                        return true;
                    }

                    stack.Push(contained);
                }
            }
        }

        return false;
    }

    #endregion

    #region UI

    private void RefreshUi(Entity<SpyContractsComponent> ent, EntityUid? user)
    {
        if (!TryComp<StoreComponent>(ent.Owner, out var store))
            return;

        var listings = user != null
            ? _store.GetAvailableListings(user.Value, ent.Owner, store).ToHashSet()
            : store.LastAvailableListings;

        Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> balance = new();
        foreach (var supported in store.CurrencyWhitelist)
            balance[supported] = store.Balance.TryGetValue(supported, out var value) ? value : FixedPoint2.Zero;

        var comp = ent.Comp;
        var board = new List<SpyContractInfo>();
        foreach (var contractId in comp.Board)
        {
            if (!_proto.TryIndex(contractId, out var contract))
                continue;
            board.Add(new SpyContractInfo(contractId, GetReward(contract, comp.Reputation)));
        }

        SpyActiveContractInfo? active = null;
        if (comp.ActiveContract != null)
        {
            var progress = 0f;
            if (comp.SurveillanceActive && _proto.TryIndex(comp.ActiveContract.Value, out var activeContract) && activeContract.Duration > 0f)
                progress = Math.Clamp(comp.Accumulated / activeContract.Duration, 0f, 1f);

            active = new SpyActiveContractInfo(comp.ActiveContract.Value, comp.ActiveReward, progress);
        }

        var nextRotation = (float) Math.Max(0, (comp.NextRotation - _timing.CurTime).TotalSeconds);

        var state = new SpyUplinkUpdateState(listings, balance, board, active, comp.Reputation, nextRotation);
        _ui.SetUiState(ent.Owner, StoreUiKey.Key, state);
    }

    #endregion
}
