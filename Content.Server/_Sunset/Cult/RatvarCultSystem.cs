using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.StationEvents;
using Content.Shared._Sunset.Cult;
using Content.Shared.Actions;
using Content.Shared.Charges.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Emp;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Radio;
using Content.Shared.Radio.Components;
using Content.Shared.Rejuvenate;
using Content.Shared._Sunset.MartialArts.Components;
using Content.Shared.Speech.Muting;
using Content.Shared.Station;
using Content.Shared.Stunnable;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - see RatvarCultistComponent/RatvarCultLeaderComponent. Handles the Rite of Offering
/// (conversion, ported from tgstation's /obj/effect/rune/convert), the private "Commune" channel, and
/// the final summoning rite (both rites share the same rune-based "N cultists must hold this ground"
/// shape - see RatvarConvertRuneComponent/RatvarSummonRuneComponent).
/// </summary>
public sealed class RatvarCultSystem : SharedRatvarCultSystem
{
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedStationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly EventManagerSystem _eventManager = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedEmpSystem _emp = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    private static readonly SoundSpecifier CommuneSound = new SoundPathSpecifier("/Audio/Effects/radpulse1.ogg");
    private static readonly ProtoId<DamageTypePrototype> BurnDamageType = "Heat";
    private static readonly ProtoId<DamageTypePrototype> SlashDamageType = "Slash";

    private readonly Dictionary<EntityUid, TimeSpan> _summonChanneling = new();
    private readonly Dictionary<EntityUid, TimeSpan> _convertChanneling = new();
    private readonly Dictionary<EntityUid, TimeSpan> _bloodBoilChanneling = new();
    private readonly Dictionary<EntityUid, TimeSpan> _apocalypseChanneling = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultConvertActionEvent>(OnScribeConvert);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultCommuneActionEvent>(OnCommune);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultScribeSummonActionEvent>(OnScribeSummon);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultScribeWallActionEvent>(OnScribeWall);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultScribeBloodBoilActionEvent>(OnScribeBloodBoil);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultScribeTeleportActionEvent>(OnScribeTeleport);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultScribeRaiseDeadActionEvent>(OnScribeRaiseDead);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultScribeApocalypseActionEvent>(OnScribeApocalypse);

        SubscribeLocalEvent<RatvarConvertRuneComponent, InteractHandEvent>(OnConvertRuneInteractHand);
        SubscribeLocalEvent<RatvarSummonRuneComponent, InteractHandEvent>(OnSummonRuneInteractHand);
        SubscribeLocalEvent<RatvarWallRuneComponent, InteractHandEvent>(OnWallRuneInteractHand);
        SubscribeLocalEvent<RatvarBloodBoilRuneComponent, InteractHandEvent>(OnBloodBoilRuneInteractHand);
        SubscribeLocalEvent<RatvarTeleportRuneComponent, InteractHandEvent>(OnTeleportRuneInteractHand);
        SubscribeLocalEvent<RatvarRaiseDeadRuneComponent, InteractHandEvent>(OnRaiseDeadRuneInteractHand);
        SubscribeLocalEvent<RatvarApocalypseRuneComponent, InteractHandEvent>(OnApocalypseRuneInteractHand);

        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultPrepareBloodMagicActionEvent>(OnPrepareBloodMagic);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultPrepareBloodMagicDoAfterEvent>(OnPrepareBloodMagicDoAfter);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultBloodStunActionEvent>(OnBloodStun);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultBloodEmpActionEvent>(OnBloodEmp);
        SubscribeLocalEvent<RatvarCultistComponent, RatvarCultBloodDaggerActionEvent>(OnBloodDagger);
    }

    #region Rite of Offering (conversion)

    private void OnScribeConvert(Entity<RatvarCultistComponent> ent, ref RatvarCultConvertActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Spawn("RatvarConvertRune", args.Target);
        _popup.PopupClient(Loc.GetString("ratvar-cult-scribe-convert-success"), ent, ent);
    }

    /// <summary>
    /// Porting tg's can_invoke/invoke for /obj/effect/rune/convert: a cultist can only start the rite if
    /// an incapacitated non-cultist is already lying on the rune - unlike the summon rune, which can be
    /// invoked immediately, this one needs a victim present before it'll even begin channeling.
    /// </summary>
    private void OnConvertRuneInteractHand(Entity<RatvarConvertRuneComponent> ent, ref InteractHandEvent args)
    {
        if (ent.Comp.Channeling || !HasComp<RatvarCultistComponent>(args.User))
            return;

        if (FindOfferingVictim(ent) is not { } victim)
        {
            _popup.PopupClient(Loc.GetString("ratvar-cult-convert-no-victim"), args.User, args.User);
            return;
        }

        ent.Comp.Channeling = true;
        ent.Comp.Victim = victim;
        _convertChanneling[ent.Owner] = _timing.CurTime + ent.Comp.ChannelTime;

        _popup.PopupEntity(Loc.GetString("ratvar-cult-rite-started", ("count", ent.Comp.RequiredCultists)), ent, PopupType.LargeCaution);
    }

    private EntityUid? FindOfferingVictim(Entity<RatvarConvertRuneComponent> rune)
    {
        foreach (var candidate in _lookup.GetEntitiesInRange<MobStateComponent>(Transform(rune).Coordinates, 0.5f))
        {
            if (HasComp<RatvarCultistComponent>(candidate) || !_mobState.IsIncapacitated(candidate))
                continue;

            if (!_mind.TryGetMind(candidate, out _, out _))
                continue;

            return candidate;
        }

        return null;
    }

    /// <summary>
    /// Porting tg's /obj/effect/rune/convert/invoke branch: "if(new_convertee.stat != DEAD && is_convertable)
    /// do_convert() else do_sacrifice()" - a victim who's still alive (just knocked out/cuffed) when the
    /// rite completes gets converted; one who's dead by then gets sacrificed toward the Raise Dead
    /// budget instead (see RatvarRaiseDeadRuneComponent).
    /// </summary>
    private void CompleteConvertRite(EntityUid rune, RatvarConvertRuneComponent comp)
    {
        QueueDel(rune);

        if (comp.Victim is not { } target || !_mind.TryGetMind(target, out var mindId, out _) || HasComp<RatvarCultistComponent>(target))
            return;

        if (_mobState.IsDead(target))
        {
            var ruleQueryDead = EntityQueryEnumerator<RatvarCultRuleComponent>();
            while (ruleQueryDead.MoveNext(out _, out var ruleComp))
                ruleComp.SacrificeCount++;

            _popup.PopupEntity(Loc.GetString("ratvar-cult-sacrifice-accepted"), target, PopupType.LargeCaution);
            return;
        }

        EnsureComp<RatvarCultistComponent>(target);
        EnsureComp<IntrinsicRadioReceiverComponent>(target);
        var activeRadio = EnsureComp<ActiveRadioComponent>(target);
        activeRadio.Channels.Add("RatvarCult");

        _role.MindAddRole(mindId, "MindRoleRatvarCultist");
        _stationSpawning.EquipStartingGear(target, "RatvarCultistGear");

        var ruleQuery = EntityQueryEnumerator<RatvarCultRuleComponent>();
        while (ruleQuery.MoveNext(out _, out var ruleComp))
            ruleComp.ConvertedCount++;

        _popup.PopupEntity(Loc.GetString("ratvar-cult-convert-success-self"), target, target, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("ratvar-cult-convert-success-others", ("target", target)), target, Filter.PvsExcept(target), true, PopupType.MediumCaution);
    }

    #endregion

    #region Commune

    private void OnCommune(Entity<RatvarCultistComponent> ent, ref RatvarCultCommuneActionEvent args)
    {
        args.Handled = true;

        var speaker = Identity.Name(ent, EntityManager);
        var query = EntityQueryEnumerator<RatvarCultistComponent, MobStateComponent>();
        while (query.MoveNext(out var uid, out _, out var mobState))
        {
            if (!_mobState.IsAlive(uid, mobState))
                continue;

            _popup.PopupEntity(Loc.GetString("ratvar-cult-commune-message", ("speaker", speaker)), uid, uid, PopupType.Medium);
            _audio.PlayEntity(CommuneSound, uid, uid);
        }
    }

    #endregion

    #region Summon rite

    private void OnScribeSummon(Entity<RatvarCultistComponent> ent, ref RatvarCultScribeSummonActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Spawn("RatvarSummonRune", args.Target);
        _popup.PopupClient(Loc.GetString("ratvar-cult-scribe-summon-success"), ent, ent);
    }

    private void OnSummonRuneInteractHand(Entity<RatvarSummonRuneComponent> ent, ref InteractHandEvent args)
    {
        if (ent.Comp.Channeling || !HasComp<RatvarCultistComponent>(args.User))
            return;

        ent.Comp.Channeling = true;
        _summonChanneling[ent.Owner] = _timing.CurTime + ent.Comp.ChannelTime;

        _popup.PopupEntity(Loc.GetString("ratvar-cult-rite-started", ("count", ent.Comp.RequiredCultists)), ent, PopupType.LargeCaution);
    }

    private void CompleteSummonRite(EntityUid rune, RatvarSummonRuneComponent comp)
    {
        var coords = Transform(rune).Coordinates;
        Spawn("MobNarsieSpawn", coords);

        var ruleQuery = EntityQueryEnumerator<RatvarCultRuleComponent>();
        while (ruleQuery.MoveNext(out _, out var ruleComp))
            ruleComp.RatvarSummoned = true;

        QueueDel(rune);
    }

    #endregion

    #region Barrier rune

    private void OnScribeWall(Entity<RatvarCultistComponent> ent, ref RatvarCultScribeWallActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Spawn("RatvarWallRune", args.Target);
        _popup.PopupClient(Loc.GetString("ratvar-cult-scribe-wall-success"), ent, ent);
    }

    /// <summary>
    /// Porting tg's /obj/effect/rune/wall/invoke: toggles a barrier over the rune each time it's
    /// invoked, and nicks the invoker for a little brute damage (tg: 2 damage to a random arm).
    /// </summary>
    private void OnWallRuneInteractHand(Entity<RatvarWallRuneComponent> ent, ref InteractHandEvent args)
    {
        if (!HasComp<RatvarCultistComponent>(args.User))
            return;

        if (ent.Comp.Barrier is { } existing && !TerminatingOrDeleted(existing))
        {
            QueueDel(existing);
            ent.Comp.Barrier = null;
            _popup.PopupEntity(Loc.GetString("ratvar-cult-wall-lowered"), ent, PopupType.Medium);
        }
        else
        {
            ent.Comp.Barrier = Spawn("RatvarCultBarrier", Transform(ent).Coordinates);
            _popup.PopupEntity(Loc.GetString("ratvar-cult-wall-raised"), ent, PopupType.Medium);
        }

        _damageable.TryChangeDamage(args.User, new DamageSpecifier(_proto.Index(BurnDamageType), 2), ignoreResistances: true);
    }

    #endregion

    #region Boiling Blood rune

    private void OnScribeBloodBoil(Entity<RatvarCultistComponent> ent, ref RatvarCultScribeBloodBoilActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Spawn("RatvarBloodBoilRune", args.Target);
        _popup.PopupClient(Loc.GetString("ratvar-cult-scribe-boil-success"), ent, ent);
    }

    private void OnBloodBoilRuneInteractHand(Entity<RatvarBloodBoilRuneComponent> ent, ref InteractHandEvent args)
    {
        if (ent.Comp.Channeling || !HasComp<RatvarCultistComponent>(args.User))
            return;

        ent.Comp.Channeling = true;
        _bloodBoilChanneling[ent.Owner] = _timing.CurTime + ent.Comp.ChannelTime;

        _popup.PopupEntity(Loc.GetString("ratvar-cult-rite-started", ("count", ent.Comp.RequiredCultists)), ent, PopupType.LargeCaution);
    }

    /// <summary>
    /// Porting tg's /obj/effect/rune/blood_boil/invoke: burns every non-cultist who can see the rune.
    /// Simplified from tg's 3-tick animated burn (25 dmg x3, escalating) into one lump application.
    /// </summary>
    private void CompleteBloodBoilRite(EntityUid rune, RatvarBloodBoilRuneComponent comp)
    {
        var coords = Transform(rune).Coordinates;
        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(coords, comp.Range))
        {
            if (HasComp<RatvarCultistComponent>(target) || _mobState.IsDead(target))
                continue;

            _damageable.TryChangeDamage(target.Owner, new DamageSpecifier(_proto.Index(BurnDamageType), comp.Damage));
            _popup.PopupEntity(Loc.GetString("ratvar-cult-blood-boils"), target.Owner, target.Owner, PopupType.LargeCaution);
        }

        QueueDel(rune);
    }

    #endregion

    #region Teleport rune

    private void OnScribeTeleport(Entity<RatvarCultistComponent> ent, ref RatvarCultScribeTeleportActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Spawn("RatvarTeleportRune", args.Target);
        _popup.PopupClient(Loc.GetString("ratvar-cult-scribe-teleport-success"), ent, ent);
    }

    /// <summary>
    /// Porting tg's /obj/effect/rune/teleport/invoke: warps everything above this rune to another
    /// linked teleport rune. Real tg lets the invoker pick which one from a list - simplified here to
    /// "the nearest other one" instead of adding a second selection UI on top of the ritual wheel.
    /// </summary>
    private void OnTeleportRuneInteractHand(Entity<RatvarTeleportRuneComponent> ent, ref InteractHandEvent args)
    {
        if (!HasComp<RatvarCultistComponent>(args.User))
            return;

        var coords = Transform(ent).Coordinates;
        EntityUid? nearest = null;
        var nearestDist = float.MaxValue;

        var query = EntityQueryEnumerator<RatvarTeleportRuneComponent, TransformComponent>();
        while (query.MoveNext(out var otherUid, out _, out var otherXform))
        {
            if (otherUid == ent.Owner)
                continue;

            var dist = (otherXform.Coordinates.Position - coords.Position).LengthSquared();
            if (dist < nearestDist)
            {
                nearestDist = dist;
                nearest = otherUid;
            }
        }

        if (nearest is not { } destination)
        {
            _popup.PopupClient(Loc.GetString("ratvar-cult-teleport-no-target"), args.User, args.User);
            return;
        }

        var destCoords = Transform(destination).Coordinates;
        var movedAnything = false;

        foreach (var movable in _lookup.GetEntitiesInRange<MobStateComponent>(coords, 0.5f))
        {
            _xform.SetCoordinates(movable, destCoords);
            movedAnything = true;
        }

        if (movedAnything)
            _popup.PopupEntity(Loc.GetString("ratvar-cult-teleport-success"), destination, PopupType.LargeCaution);
    }

    #endregion

    #region Raise Dead rune

    private void OnScribeRaiseDead(Entity<RatvarCultistComponent> ent, ref RatvarCultScribeRaiseDeadActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Spawn("RatvarRaiseDeadRune", args.Target);
        _popup.PopupClient(Loc.GetString("ratvar-cult-scribe-raise-dead-success"), ent, ent);
    }

    /// <summary>
    /// Porting tg's /obj/effect/rune/raise_dead/invoke: revives a dead cultist lying on the rune,
    /// spending <see cref="RatvarRaiseDeadRuneComponent.SoulsRequired"/> sacrifices banked by the Rune
    /// of Offering (tg: "For each three bodies sacrificed to the dark patron, one body will be mended").
    /// </summary>
    private void OnRaiseDeadRuneInteractHand(Entity<RatvarRaiseDeadRuneComponent> ent, ref InteractHandEvent args)
    {
        if (!HasComp<RatvarCultistComponent>(args.User))
            return;

        EntityUid? deadCultist = null;
        foreach (var candidate in _lookup.GetEntitiesInRange<RatvarCultistComponent>(Transform(ent).Coordinates, 0.5f))
        {
            if (_mobState.IsDead(candidate))
            {
                deadCultist = candidate;
                break;
            }
        }

        if (deadCultist is not { } target)
        {
            _popup.PopupClient(Loc.GetString("ratvar-cult-raise-dead-no-target"), args.User, args.User);
            return;
        }

        RatvarCultRuleComponent? ruleComp = null;
        var ruleQuery = EntityQueryEnumerator<RatvarCultRuleComponent>();
        if (ruleQuery.MoveNext(out _, out var foundRule))
            ruleComp = foundRule;

        var available = (ruleComp?.SacrificeCount ?? 0) - (ruleComp?.SacrificesUsed ?? 0);
        if (ruleComp == null || available < ent.Comp.SoulsRequired)
        {
            _popup.PopupClient(Loc.GetString("ratvar-cult-raise-dead-not-enough-souls", ("required", ent.Comp.SoulsRequired), ("available", Math.Max(available, 0))), args.User, args.User);
            return;
        }

        ruleComp.SacrificesUsed += ent.Comp.SoulsRequired;

        RaiseLocalEvent(target, new RejuvenateEvent());
        _popup.PopupEntity(Loc.GetString("ratvar-cult-raise-dead-success"), target, PopupType.LargeCaution);
        QueueDel(ent.Owner);
    }

    #endregion

    #region Apocalypse rune

    private void OnScribeApocalypse(Entity<RatvarCultistComponent> ent, ref RatvarCultScribeApocalypseActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        Spawn("RatvarApocalypseRune", args.Target);
        _popup.PopupClient(Loc.GetString("ratvar-cult-scribe-apocalypse-success"), ent, ent);
    }

    private void OnApocalypseRuneInteractHand(Entity<RatvarApocalypseRuneComponent> ent, ref InteractHandEvent args)
    {
        if (ent.Comp.Channeling || !HasComp<RatvarCultistComponent>(args.User))
            return;

        ent.Comp.Channeling = true;
        _apocalypseChanneling[ent.Owner] = _timing.CurTime + ent.Comp.ChannelTime;

        _popup.PopupEntity(Loc.GetString("ratvar-cult-rite-started", ("count", ent.Comp.RequiredCultists)), ent, PopupType.LargeCaution);
    }

    /// <summary>
    /// Porting tg's /obj/effect/rune/apocalypse/invoke: paralyzes everyone nearby and unleashes chaos.
    /// Real tg consumes a summoning site and forces a chaos event weighted by cult desperation and
    /// applies temporary "everyone looks like a cultist/construct" paranoia illusions - simplified here
    /// to a paralyze burst + one forced random station event, since summon-site gating and the
    /// per-player illusion system are Nar'Sie-specific systems this fork doesn't have.
    /// </summary>
    private void CompleteApocalypseRite(EntityUid rune, RatvarApocalypseRuneComponent comp)
    {
        var coords = Transform(rune).Coordinates;

        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(coords, comp.Range))
        {
            if (!HasComp<RatvarCultistComponent>(target))
                _stun.TryAddParalyzeDuration(target.Owner, TimeSpan.FromSeconds(3));
        }

#pragma warning disable CS0618 // no scheduler-scoped event table available in this context; the unweighted overload is still functional
        _eventManager.RunRandomEvent();
#pragma warning restore CS0618

        QueueDel(rune);
    }

    #endregion

    #region Blood Magic

    private void OnPrepareBloodMagic(Entity<RatvarCultistComponent> ent, ref RatvarCultPrepareBloodMagicActionEvent args)
    {
        if (args.Handled || HasComp<RatvarBloodMagicPreparedComponent>(ent))
        {
            _popup.PopupClient(Loc.GetString("ratvar-cult-blood-magic-already-prepared"), ent, ent);
            return;
        }

        args.Handled = true;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, TimeSpan.FromSeconds(6), new RatvarCultPrepareBloodMagicDoAfterEvent(), ent)
        {
            BreakOnMove = true,
            BreakOnDamage = false,
            NeedHand = false,
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs))
            _popup.PopupClient(Loc.GetString("ratvar-cult-blood-magic-carving"), ent, ent);
    }

    /// <summary>
    /// Porting tg's carve-and-bleed cost for preparing blood magic (scaled down from tg's 40 damage for
    /// a full-cost carve). Grants a one-time starter kit of three spells instead of tg's pick-one-at-a-
    /// time-from-a-list system (see RatvarCultPrepareBloodMagicActionEvent for why).
    /// </summary>
    private void OnPrepareBloodMagicDoAfter(Entity<RatvarCultistComponent> ent, ref RatvarCultPrepareBloodMagicDoAfterEvent args)
    {
        if (args.Cancelled || HasComp<RatvarBloodMagicPreparedComponent>(ent))
            return;

        EnsureComp<RatvarBloodMagicPreparedComponent>(ent);
        _damageable.TryChangeDamage(ent.Owner, new DamageSpecifier(_proto.Index(SlashDamageType), 15), ignoreResistances: true);

        _actions.AddAction(ent, "ActionRatvarCultBloodStun");
        _actions.AddAction(ent, "ActionRatvarCultBloodEmp");
        _actions.AddAction(ent, "ActionRatvarCultBloodDagger");

        _popup.PopupEntity(Loc.GetString("ratvar-cult-blood-magic-prepared"), ent, ent, PopupType.LargeCaution);
    }

    private void OnBloodStun(Entity<RatvarCultistComponent> ent, ref RatvarCultBloodStunActionEvent args)
    {
        if (args.Handled || !TryComp<MobStateComponent>(args.Target, out _))
            return;

        if (!_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 1.5f))
            return;

        if (!_charges.TryUseCharge(args.Action.Owner))
            return;

        args.Handled = true;

        _stun.TryAddParalyzeDuration(args.Target, TimeSpan.FromSeconds(4));
        EnsureComp<MutedComponent>(args.Target);
        var tempMute = EnsureComp<TemporaryMuteComponent>(args.Target);
        tempMute.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(10);

        _popup.PopupEntity(Loc.GetString("ratvar-cult-blood-stun-success", ("target", args.Target)), ent, PopupType.MediumCaution);

        if (_charges.IsEmpty(args.Action.Owner))
            _actions.RemoveAction(ent.Owner, args.Action.Owner);
    }

    private void OnBloodEmp(Entity<RatvarCultistComponent> ent, ref RatvarCultBloodEmpActionEvent args)
    {
        if (args.Handled || !_charges.TryUseCharge(args.Action.Owner))
            return;

        args.Handled = true;

        _emp.EmpPulse(Transform(ent).Coordinates, 5f, 100f, TimeSpan.FromSeconds(15), ent);
        _popup.PopupEntity(Loc.GetString("ratvar-cult-blood-emp-success"), ent, PopupType.MediumCaution);

        if (_charges.IsEmpty(args.Action.Owner))
            _actions.RemoveAction(ent.Owner, args.Action.Owner);
    }

    private void OnBloodDagger(Entity<RatvarCultistComponent> ent, ref RatvarCultBloodDaggerActionEvent args)
    {
        if (args.Handled || !_charges.TryUseCharge(args.Action.Owner))
            return;

        args.Handled = true;

        var dagger = Spawn("RatvarCultDagger", Transform(ent).Coordinates);
        _hands.TryPickupAnyHand(ent, dagger);
        _popup.PopupEntity(Loc.GetString("ratvar-cult-blood-dagger-success"), ent, PopupType.Medium);

        if (_charges.IsEmpty(args.Action.Owner))
            _actions.RemoveAction(ent.Owner, args.Action.Owner);
    }

    #endregion

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateSummonChanneling();
        UpdateConvertChanneling();
        UpdateBloodBoilChanneling();
        UpdateApocalypseChanneling();
    }

    private void UpdateSummonChanneling()
    {
        if (_summonChanneling.Count == 0)
            return;

        var finished = new List<EntityUid>();

        foreach (var (rune, endTime) in _summonChanneling)
        {
            if (!TryComp<RatvarSummonRuneComponent>(rune, out var comp))
            {
                finished.Add(rune);
                continue;
            }

            if (CountLivingCultistsNearby(rune, comp.Range) < comp.RequiredCultists)
            {
                comp.Channeling = false;
                finished.Add(rune);
                _popup.PopupEntity(Loc.GetString("ratvar-cult-rite-interrupted"), rune, PopupType.MediumCaution);
                continue;
            }

            if (_timing.CurTime < endTime)
                continue;

            finished.Add(rune);
            CompleteSummonRite(rune, comp);
        }

        foreach (var rune in finished)
            _summonChanneling.Remove(rune);
    }

    private void UpdateConvertChanneling()
    {
        if (_convertChanneling.Count == 0)
            return;

        var finished = new List<EntityUid>();

        foreach (var (rune, endTime) in _convertChanneling)
        {
            if (!TryComp<RatvarConvertRuneComponent>(rune, out var comp))
            {
                finished.Add(rune);
                continue;
            }

            var victimStillOffered = comp.Victim is { } victim && !TerminatingOrDeleted(victim) && _mobState.IsIncapacitated(victim);

            if (!victimStillOffered || CountLivingCultistsNearby(rune, comp.Range) < comp.RequiredCultists)
            {
                comp.Channeling = false;
                comp.Victim = null;
                finished.Add(rune);
                _popup.PopupEntity(Loc.GetString("ratvar-cult-rite-interrupted"), rune, PopupType.MediumCaution);
                continue;
            }

            if (_timing.CurTime < endTime)
                continue;

            finished.Add(rune);
            CompleteConvertRite(rune, comp);
        }

        foreach (var rune in finished)
            _convertChanneling.Remove(rune);
    }

    private void UpdateBloodBoilChanneling()
    {
        if (_bloodBoilChanneling.Count == 0)
            return;

        var finished = new List<EntityUid>();

        foreach (var (rune, endTime) in _bloodBoilChanneling)
        {
            if (!TryComp<RatvarBloodBoilRuneComponent>(rune, out var comp))
            {
                finished.Add(rune);
                continue;
            }

            if (CountLivingCultistsNearby(rune, comp.Range) < comp.RequiredCultists)
            {
                comp.Channeling = false;
                finished.Add(rune);
                _popup.PopupEntity(Loc.GetString("ratvar-cult-rite-interrupted"), rune, PopupType.MediumCaution);
                continue;
            }

            if (_timing.CurTime < endTime)
                continue;

            finished.Add(rune);
            CompleteBloodBoilRite(rune, comp);
        }

        foreach (var rune in finished)
            _bloodBoilChanneling.Remove(rune);
    }

    private void UpdateApocalypseChanneling()
    {
        if (_apocalypseChanneling.Count == 0)
            return;

        var finished = new List<EntityUid>();

        foreach (var (rune, endTime) in _apocalypseChanneling)
        {
            if (!TryComp<RatvarApocalypseRuneComponent>(rune, out var comp))
            {
                finished.Add(rune);
                continue;
            }

            if (CountLivingCultistsNearby(rune, comp.Range) < comp.RequiredCultists)
            {
                comp.Channeling = false;
                finished.Add(rune);
                _popup.PopupEntity(Loc.GetString("ratvar-cult-rite-interrupted"), rune, PopupType.MediumCaution);
                continue;
            }

            if (_timing.CurTime < endTime)
                continue;

            finished.Add(rune);
            CompleteApocalypseRite(rune, comp);
        }

        foreach (var rune in finished)
            _apocalypseChanneling.Remove(rune);
    }

    private int CountLivingCultistsNearby(EntityUid rune, float range)
    {
        var livingNearby = 0;
        foreach (var cultist in _lookup.GetEntitiesInRange<RatvarCultistComponent>(Transform(rune).Coordinates, range))
        {
            if (_mobState.IsAlive(cultist))
                livingNearby++;
        }

        return livingNearby;
    }
}
