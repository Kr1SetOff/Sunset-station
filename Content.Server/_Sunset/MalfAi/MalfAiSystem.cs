using Content.Server.AlertLevel;
using Content.Server.Chat.Systems;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Nuke;
using Content.Server.Popups;
using Content.Server.Power.Components;
using Content.Server.RoundEnd;
using Content.Server.Station.Systems;
using Content.Server.Store.Systems;
using Content.Shared._Sunset.MalfAi;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.DoAfter;
using Content.Shared.Doors.Components;
using Content.Shared.Doors.Systems;
using Content.Shared.Emag.Components;
using Content.Shared.Emag.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Gibbing;
using Content.Shared.Light.Components;
using Content.Shared.Light.EntitySystems;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Content.Shared.Power.Components;
using Content.Shared.RCD.Components;
using Content.Shared.Silicons.Laws.Components;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Store.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.MalfAi;

/// <summary>
/// 🌇Sunset🌇 - the Malfunctioning AI's abilities: APC hacking for CPU, and the module kit compiled
/// from the best of the SS13 builds (tg module list + Paradise-style APC-hack economy) - Doomsday
/// Device, Hostile Station Lockdown, Machine Overload, Blackout, Destroy RCDs and Targeted Safeties
/// Override. Antag wiring (store/laws/briefing) lives in <see cref="MalfAiRuleSystem"/>.
/// </summary>
public sealed class MalfAiSystem : EntitySystem
{
    [Dependency] private AlertLevelSystem _alertLevel = default!;
    [Dependency] private ChatSystem _chat = default!;
    [Dependency] private ExplosionSystem _explosion = default!;
    [Dependency] private GibbingSystem _gibbing = default!;
    [Dependency] private ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private NukeSystem _nuke = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private PopupSystem _popup = default!;
    [Dependency] private RoundEndSystem _roundEnd = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedDoorSystem _doors = default!;
    [Dependency] private SharedPoweredLightSystem _poweredLight = default!;
    [Dependency] private SharedStationAiSystem _stationAi = default!;
    [Dependency] private StationSystem _station = default!;
    [Dependency] private StoreSystem _store = default!;

    private static readonly SoundPathSpecifier OverloadWarningSound = new("/Audio/Machines/warning_buzzer.ogg");
    private static readonly SoundCollectionSpecifier SparkSound = new("sparks");

    private static readonly int[] DoomsdayWarningMarks = { 300, 240, 180, 120, 60, 30, 10 };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MalfAiComponent, ComponentShutdown>(OnMalfShutdown);
        SubscribeLocalEvent<MalfAiComponent, MalfAiOpenModulesEvent>(OnOpenModules);
        SubscribeLocalEvent<MalfAiComponent, MalfAiHackApcActionEvent>(OnHackApc);
        SubscribeLocalEvent<MalfAiComponent, MalfAiHackApcDoAfterEvent>(OnHackApcDoAfter);
        SubscribeLocalEvent<MalfAiComponent, MalfAiDoomsdayEvent>(OnDoomsday);
        SubscribeLocalEvent<MalfAiComponent, MalfAiLockdownEvent>(OnLockdown);
        SubscribeLocalEvent<MalfAiComponent, MalfAiBlackoutEvent>(OnBlackout);
        SubscribeLocalEvent<MalfAiComponent, MalfAiDestroyRcdsEvent>(OnDestroyRcds);
        SubscribeLocalEvent<MalfAiComponent, MalfAiOverloadMachineEvent>(OnOverloadMachine);
        SubscribeLocalEvent<MalfAiComponent, MalfAiOverrideSafetiesEvent>(OnOverrideSafeties);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;

        var query = EntityQueryEnumerator<MalfAiComponent>();
        while (query.MoveNext(out var uid, out var malf))
        {
            UpdateCpuIncome(uid, malf, now);
            UpdateDoomsday(uid, malf, now);
            UpdateLockdown(malf, now);
        }

        var overloadQuery = EntityQueryEnumerator<MalfAiOverloadingComponent>();
        while (overloadQuery.MoveNext(out var uid, out var overloading))
        {
            if (now < overloading.ExplodeAt)
                continue;

            _explosion.QueueExplosion(uid, "Default", 20f, 3f, 5f, user: overloading.Cause);
            QueueDel(uid);
        }
    }

    /// <summary>
    /// The AI brain being destroyed (core blown up, etc.) mid-doomsday must still abort the
    /// countdown cleanly - otherwise the station stays locked on delta with a countdown that
    /// never resolves.
    /// </summary>
    private void OnMalfShutdown(Entity<MalfAiComponent> ent, ref ComponentShutdown args)
    {
        if (ent.Comp.DoomsdayActive && ent.Comp.DoomsdayStation is { } station)
        {
            ent.Comp.DoomsdayActive = false;
            _alertLevel.SetLevel(station, "red", true, true, true, false);
            Announce(station, Loc.GetString("malf-ai-doomsday-aborted"));
        }

        // Never leave the station bolted shut because the AI died mid-lockdown.
        UpdateLockdown(ent.Comp, TimeSpan.MaxValue);
    }

    #region Economy

    private void UpdateCpuIncome(EntityUid uid, MalfAiComponent malf, TimeSpan now)
    {
        if (now < malf.NextCpuTick)
            return;

        malf.NextCpuTick = now + TimeSpan.FromMinutes(1);

        // Carded/coreless AIs earn nothing - same idea as tg only letting modules work from the core.
        if (!_stationAi.TryGetCore(uid, out _))
            return;

        var income = malf.PassiveCpuPerMinute + malf.HackedApcs * malf.CpuPerHackedApcPerMinute;
        AddCpu(uid, income);
    }

    private void AddCpu(EntityUid uid, float amount)
    {
        if (!TryComp<StoreComponent>(uid, out var store))
            return;

        _store.TryAddCurrency(
            new Dictionary<string, FixedPoint2> { { MalfAiRuleSystem.CpuCurrency, FixedPoint2.New(amount) } },
            uid,
            store);
    }

    #endregion

    #region APC hacking

    private void OnHackApc(Entity<MalfAiComponent> ent, ref MalfAiHackApcActionEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        // Validated server-side because ApcComponent is server-only and can't go in the
        // action's client-checked whitelist.
        if (!HasComp<ApcComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-not-an-apc"), target, ent, PopupType.Medium);
            return;
        }

        if (HasComp<MalfAiHackedApcComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-apc-already-hacked"), target, ent, PopupType.Medium);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, ent.Comp.HackDuration, new MalfAiHackApcDoAfterEvent(), ent, target)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            NeedHand = false,
            RequireCanInteract = false,
            // The AI brain sits in its core across the station from the APC - the default 1.5
            // tile distance check would cancel the do-after on the very first tick.
            DistanceThreshold = null,
            // Don't draw a progress bar over the AI core for everyone to see.
            Hidden = true,
        };

        if (!_doAfter.TryStartDoAfter(doAfterArgs))
            return;

        _popup.PopupEntity(Loc.GetString("malf-ai-apc-hack-started"), target, ent, PopupType.Medium);
        args.Handled = true;
    }

    private void OnHackApcDoAfter(Entity<MalfAiComponent> ent, ref MalfAiHackApcDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || args.Args.Target is not { } target)
            return;

        if (HasComp<MalfAiHackedApcComponent>(target))
            return;

        var hacked = EnsureComp<MalfAiHackedApcComponent>(target);
        hacked.HackedBy = ent.Owner;

        // The crew-visible tell (and interface lock): the APC takes the emagged state, standing in
        // for tg's blue screen with blinking red lights.
        TryMalfEmag(ent, target);

        ent.Comp.HackedApcs++;
        AddCpu(ent, ent.Comp.CpuOnHack);

        _audio.PlayPvs(SparkSound, target);
        _popup.PopupEntity(Loc.GetString("malf-ai-apc-hack-finished", ("count", ent.Comp.HackedApcs)), target, ent, PopupType.LargeCaution);
        _adminLogger.Add(LogType.Action, LogImpact.High,
            $"{ToPrettyString(ent.Owner):player} (malf AI) hacked APC {ToPrettyString(target):target} (total {ent.Comp.HackedApcs})");

        args.Handled = true;
    }

    #endregion

    #region Modules

    private void OnOpenModules(Entity<MalfAiComponent> ent, ref MalfAiOpenModulesEvent args)
    {
        if (!TryComp<StoreComponent>(ent, out var store))
            return;

        _store.ToggleUi(ent, ent, store);
        args.Handled = true;
    }

    private void OnDoomsday(Entity<MalfAiComponent> ent, ref MalfAiDoomsdayEvent args)
    {
        if (args.Handled || ent.Comp.DoomsdayActive)
            return;

        if (ent.Comp.HackedApcs < ent.Comp.DoomsdayRequiredApcs)
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-doomsday-not-enough-apcs",
                ("hacked", ent.Comp.HackedApcs), ("required", ent.Comp.DoomsdayRequiredApcs)), ent, ent);
            return;
        }

        if (!_stationAi.TryGetCore(ent.Owner, out var core) || core.Comp == null)
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-doomsday-no-core"), ent, ent);
            return;
        }

        var station = _station.GetOwningStation(core.Owner);
        if (station == null)
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-doomsday-off-station"), ent, ent);
            return;
        }

        ent.Comp.DoomsdayActive = true;
        ent.Comp.DoomsdayStation = station;
        ent.Comp.DoomsdayEndTime = _timing.CurTime + ent.Comp.DoomsdayDelay;
        ent.Comp.DoomsdayLastWarning = int.MaxValue;

        _alertLevel.SetLevel(station.Value, "delta", true, true, true, true);
        Announce(station.Value, Loc.GetString("malf-ai-doomsday-announcement",
            ("seconds", (int) ent.Comp.DoomsdayDelay.TotalSeconds)));

        _adminLogger.Add(LogType.Action, LogImpact.Extreme,
            $"{ToPrettyString(ent.Owner):player} (malf AI) activated the DOOMSDAY DEVICE on station {ToPrettyString(station.Value)}");

        args.Handled = true;
    }

    private void OnLockdown(Entity<MalfAiComponent> ent, ref MalfAiLockdownEvent args)
    {
        if (args.Handled)
            return;

        if (ent.Comp.LockdownEndTime != null)
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-lockdown-already-active"), ent, ent);
            return;
        }

        var station = GetMalfStation(ent);
        if (station == null)
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-doomsday-off-station"), ent, ent);
            return;
        }

        var count = 0;
        var query = EntityQueryEnumerator<DoorBoltComponent, DoorComponent>();
        while (query.MoveNext(out var doorUid, out var bolt, out _))
        {
            if (_station.GetOwningStation(doorUid) != station)
                continue;

            _doors.TryClose(doorUid);
            _doors.SetBoltsDown((doorUid, bolt), true);
            ent.Comp.LockdownDoors.Add(doorUid);
            count++;
        }

        ent.Comp.LockdownEndTime = _timing.CurTime + ent.Comp.LockdownDuration;

        Announce(station.Value, Loc.GetString("malf-ai-lockdown-announcement"));
        _adminLogger.Add(LogType.Action, LogImpact.High,
            $"{ToPrettyString(ent.Owner):player} (malf AI) initiated hostile station lockdown ({count} doors)");

        args.Handled = true;
    }

    private void OnBlackout(Entity<MalfAiComponent> ent, ref MalfAiBlackoutEvent args)
    {
        if (args.Handled)
            return;

        var station = GetMalfStation(ent);
        if (station == null)
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-doomsday-off-station"), ent, ent);
            return;
        }

        var count = 0;
        var query = EntityQueryEnumerator<PoweredLightComponent>();
        while (query.MoveNext(out var lightUid, out var light))
        {
            if (_station.GetOwningStation(lightUid) != station)
                continue;

            if (!_random.Prob(ent.Comp.BlackoutBreakChance))
                continue;

            if (_poweredLight.TryDestroyBulb(lightUid, light))
                count++;
        }

        _popup.PopupEntity(Loc.GetString("malf-ai-blackout-done", ("count", count)), ent, ent);
        _adminLogger.Add(LogType.Action, LogImpact.High,
            $"{ToPrettyString(ent.Owner):player} (malf AI) triggered blackout, broke {count} lights");

        args.Handled = true;
    }

    private void OnDestroyRcds(Entity<MalfAiComponent> ent, ref MalfAiDestroyRcdsEvent args)
    {
        if (args.Handled)
            return;

        var station = GetMalfStation(ent);
        if (station == null)
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-doomsday-off-station"), ent, ent);
            return;
        }

        var count = 0;
        var query = EntityQueryEnumerator<RCDComponent>();
        while (query.MoveNext(out var rcdUid, out _))
        {
            if (_station.GetOwningStation(rcdUid) != station)
                continue;

            _explosion.QueueExplosion(rcdUid, "Default", 15f, 3f, 5f, user: ent.Owner);
            QueueDel(rcdUid);
            count++;
        }

        _popup.PopupEntity(Loc.GetString("malf-ai-destroy-rcds-done", ("count", count)), ent, ent);
        _adminLogger.Add(LogType.Action, LogImpact.High,
            $"{ToPrettyString(ent.Owner):player} (malf AI) detonated {count} RCDs");

        args.Handled = true;
    }

    private void OnOverloadMachine(Entity<MalfAiComponent> ent, ref MalfAiOverloadMachineEvent args)
    {
        if (args.Handled)
            return;

        var target = args.Target;

        // Server-side validation - ApcPowerReceiverComponent is server-only, so it can't be a
        // client-checked action whitelist.
        if (!HasComp<ApcPowerReceiverComponent>(target))
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-not-a-machine"), target, ent, PopupType.Medium);
            return;
        }

        if (HasComp<MalfAiOverloadingComponent>(target))
            return;

        var overloading = EnsureComp<MalfAiOverloadingComponent>(target);
        overloading.ExplodeAt = _timing.CurTime + TimeSpan.FromSeconds(5);
        overloading.Cause = ent.Owner;

        // tg's audible warning for anyone standing next to the doomed machine.
        _audio.PlayPvs(OverloadWarningSound, target);
        _popup.PopupEntity(Loc.GetString("malf-ai-overload-warning"), target, PopupType.LargeCaution);

        _adminLogger.Add(LogType.Action, LogImpact.High,
            $"{ToPrettyString(ent.Owner):player} (malf AI) overloaded machine {ToPrettyString(target):target}");

        args.Handled = true;
    }

    private void OnOverrideSafeties(Entity<MalfAiComponent> ent, ref MalfAiOverrideSafetiesEvent args)
    {
        if (args.Handled)
            return;

        if (!TryMalfEmag(ent, args.Target))
        {
            _popup.PopupEntity(Loc.GetString("malf-ai-safeties-no-effect"), ent, ent);
            return;
        }

        _popup.PopupEntity(Loc.GetString("malf-ai-safeties-done"), ent, ent);
        _adminLogger.Add(LogType.Emag, LogImpact.High,
            $"{ToPrettyString(ent.Owner):player} (malf AI) overrode safeties on {ToPrettyString(args.Target):target}");

        args.Handled = true;
    }

    #endregion

    #region Doomsday countdown

    private void UpdateDoomsday(EntityUid uid, MalfAiComponent malf, TimeSpan now)
    {
        if (!malf.DoomsdayActive || malf.DoomsdayStation is not { } station)
            return;

        // Carding the AI (or destroying the brain outright, which kills the component with it)
        // aborts the countdown, mirroring tg's shunt/card cancel.
        if (!_stationAi.TryGetCore(uid, out _))
        {
            malf.DoomsdayActive = false;
            malf.DoomsdayStation = null;
            _alertLevel.SetLevel(station, "red", true, true, true, false);
            Announce(station, Loc.GetString("malf-ai-doomsday-aborted"));
            _adminLogger.Add(LogType.Action, LogImpact.High, $"Malf AI doomsday aborted: {ToPrettyString(uid)} lost its core");
            return;
        }

        var remaining = (int) Math.Ceiling((malf.DoomsdayEndTime - now).TotalSeconds);

        if (remaining <= 0)
        {
            DetonateDoomsday(uid, malf, station);
            return;
        }

        foreach (var mark in DoomsdayWarningMarks)
        {
            if (remaining <= mark && malf.DoomsdayLastWarning > mark)
            {
                malf.DoomsdayLastWarning = mark;
                Announce(station, Loc.GetString("malf-ai-doomsday-countdown", ("seconds", mark)));
                break;
            }
        }
    }

    private void DetonateDoomsday(EntityUid uid, MalfAiComponent malf, EntityUid station)
    {
        malf.DoomsdayActive = false;
        malf.DoomsdayStation = null;

        Announce(station, Loc.GetString("malf-ai-doomsday-detonation"));

        var killed = 0;
        var query = EntityQueryEnumerator<MobStateComponent>();
        var toKill = new List<EntityUid>();
        while (query.MoveNext(out var mobUid, out _))
        {
            // "All organic life": silicons (borgs, the AI itself) are spared, as in tg.
            if (HasComp<SiliconLawBoundComponent>(mobUid) || HasComp<StationAiHeldComponent>(mobUid))
                continue;

            if (_station.GetOwningStation(mobUid) != station)
                continue;

            toKill.Add(mobUid);
        }

        foreach (var mobUid in toKill)
        {
            _gibbing.Gib(mobUid, dropGiblets: true, user: uid);
            killed++;
        }

        // The station's own nuclear warhead goes off immediately - the doomsday IS the self-destruct
        // sequence, so the fissile charge detonates the moment the countdown hits zero. ActivateBomb
        // bypasses the nuke's own arming timer entirely.
        var nukeDetonated = false;
        var nukeQuery = EntityQueryEnumerator<Content.Server.Nuke.NukeComponent>();
        while (nukeQuery.MoveNext(out var nukeUid, out var nukeComp))
        {
            if (_station.GetOwningStation(nukeUid) != station)
                continue;

            _nuke.ActivateBomb(nukeUid, nukeComp);
            nukeDetonated = true;
            break;
        }

        _adminLogger.Add(LogType.Action, LogImpact.Extreme,
            $"Malf AI {ToPrettyString(uid):player} DOOMSDAY detonated on {ToPrettyString(station)}: {killed} organics caught in it, station nuke detonated: {nukeDetonated}");

        _roundEnd.EndRound(TimeSpan.FromSeconds(10));
    }

    #endregion

    #region Helpers

    private void UpdateLockdown(MalfAiComponent malf, TimeSpan now)
    {
        if (malf.LockdownEndTime == null || now < malf.LockdownEndTime)
            return;

        foreach (var doorUid in malf.LockdownDoors)
        {
            if (TerminatingOrDeleted(doorUid) || !TryComp<DoorBoltComponent>(doorUid, out var bolt))
                continue;

            _doors.SetBoltsDown((doorUid, bolt), false);
        }

        malf.LockdownDoors.Clear();
        malf.LockdownEndTime = null;
    }

    /// <summary>
    /// Applies the emag interaction effect without an emag item in hand, mimicking
    /// EmagSystem.TryEmagEffect's event + EmaggedComponent bookkeeping.
    /// </summary>
    private bool TryMalfEmag(EntityUid user, EntityUid target)
    {
        var ev = new GotEmaggedEvent(user, EmagType.Interaction);
        RaiseLocalEvent(target, ref ev);

        if (!ev.Handled)
            return false;

        EnsureComp<EmaggedComponent>(target, out var emagged);
        if (!ev.Repeatable)
            emagged.EmagType |= EmagType.Interaction;
        Dirty(target, emagged);

        return true;
    }

    private EntityUid? GetMalfStation(EntityUid uid)
    {
        if (!_stationAi.TryGetCore(uid, out var core))
            return null;

        return _station.GetOwningStation(core.Owner);
    }

    private void Announce(EntityUid station, string message)
    {
        _chat.DispatchStationAnnouncement(station, message,
            sender: Loc.GetString("malf-ai-announcement-sender"), colorOverride: Color.DarkRed);
    }

    #endregion
}
