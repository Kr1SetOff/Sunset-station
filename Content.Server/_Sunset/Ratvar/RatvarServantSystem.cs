using Content.Server.Mind;
using Content.Server.Popups;
using Content.Server.Roles;
using Content.Server.Stack;
using Content.Shared._Sunset.Ratvar;
using Content.Shared._Sunset.MartialArts.Components;
using Content.Shared.Actions;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Speech.Muting;
using Content.Shared.Stacks;
using Content.Shared.Stunnable;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - the real Servants of Ratvar antagonist (see RatvarServantComponent). Handles Altar
/// conversion (single acolyte, no second-invoker requirement, Mind Shield immune - unlike Nar'Sie's
/// Rite of Offering) and the clockwork slab's three built-in abilities: Stun, Teleport, Heal.
/// </summary>
public sealed class RatvarServantSystem : SharedRatvarServantSystem
{
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly RoleSystem _role = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedStackSystem _stackSystem = default!;
    [Dependency] private readonly StackSystem _stackSpawn = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;

    private static readonly ProtoId<DamageTypePrototype> BruteDamageType = "Blunt";
    private static readonly ProtoId<DamageTypePrototype> BurnDamageType = "Heat";
    private const int BeaconBrassCost = 6;
    private const string BrassStackType = "Brass";

    /// <summary>
    /// Every action a servant should have. Granted by GrantServantActions (called from
    /// OnServantStartup) for both round-start antags and Altar conversions alike - not via ActionGrant,
    /// since that only fires on MapInit and Altar-converted servants never get one.
    /// </summary>
    private static readonly EntProtoId[] ServantActions =
    {
        "ActionRatvarSlabStun",
        "ActionRatvarSlabTeleport",
        "ActionRatvarSlabHeal",
        "ActionRatvarHandOfMidas",
        "ActionRatvarPlaceBeacon",
        "ActionRatvarCraftGauntlets",
        "ActionRatvarCraftTreads",
        "ActionRatvarCraftHelmet",
        "ActionRatvarCraftCuirass",
        "ActionRatvarCraftSpear",
        "ActionRatvarCraftCogscarab",
        "ActionRatvarCraftMarauder",
        "ActionRatvarCraftGolem",
        "ActionRatvarConcealGears",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RatvarAltarComponent, InteractHandEvent>(OnAltarInteractHand);
        SubscribeLocalEvent<RatvarServantComponent, ComponentStartup>(OnServantStartup);

        SubscribeLocalEvent<RatvarServantComponent, RatvarSlabStunActionEvent>(OnSlabStun);
        SubscribeLocalEvent<RatvarServantComponent, RatvarSlabTeleportActionEvent>(OnSlabTeleport);
        SubscribeLocalEvent<RatvarServantComponent, RatvarSlabHealActionEvent>(OnSlabHeal);

        SubscribeLocalEvent<RatvarServantComponent, RatvarHandOfMidasActionEvent>(OnHandOfMidas);
        SubscribeLocalEvent<RatvarServantComponent, RatvarPlaceBeaconActionEvent>(OnPlaceBeacon);
        SubscribeLocalEvent<RatvarServantComponent, RatvarConcealGearsActionEvent>(OnConcealGears);

        SubscribeLocalEvent<RatvarServantComponent, RatvarCraftGearActionEvent>(OnCraftGear);
        SubscribeLocalEvent<RatvarServantComponent, RatvarCraftGearDoAfterEvent>(OnCraftGearDoAfter);

        SubscribeLocalEvent<RatvarServantComponent, RatvarCraftConstructActionEvent>(OnCraftConstruct);
        SubscribeLocalEvent<RatvarServantComponent, RatvarCraftConstructDoAfterEvent>(OnCraftConstructDoAfter);

        SubscribeLocalEvent<RatvarWorkshopComponent, InteractHandEvent>(OnWorkshopInteractHand);
        SubscribeLocalEvent<RatvarWorkshopComponent, RatvarCraftShardDoAfterEvent>(OnCraftShardDoAfter);
        SubscribeLocalEvent<RatvarAltarComponent, InteractUsingEvent>(OnAltarInteractUsing);
    }

    #region Altar conversion

    /// <summary>
    /// Porting the ss220.space RU wiki's description of the Altar: an incapacitated non-servant is
    /// brought to it, a single servant interacts with it, and (barring a Mind Shield implant) they
    /// convert - much simpler than Nar'Sie's 2-invoker Rite of Offering.
    /// </summary>
    private void OnAltarInteractHand(Entity<RatvarAltarComponent> ent, ref InteractHandEvent args)
    {
        if (!HasComp<RatvarServantComponent>(args.User))
            return;

        if (FindOfferingVictim(ent) is not { } victim)
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-altar-no-victim"), args.User, args.User);
            return;
        }

        if (HasComp<MindShieldComponent>(victim))
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-altar-mind-shielded"), args.User, args.User);
            return;
        }

        if (!_mind.TryGetMind(victim, out var mindId, out _))
            return;

        EnsureComp<RatvarServantComponent>(victim);
        _role.MindAddRole(mindId, "MindRoleRatvarServant");

        var ruleQuery = EntityQueryEnumerator<RatvarServantRuleComponent>();
        while (ruleQuery.MoveNext(out _, out var ruleComp))
            ruleComp.ConvertedCount++;

        _popup.PopupEntity(Loc.GetString("ratvar-servant-convert-success-self"), victim, victim, PopupType.LargeCaution);
        _popup.PopupEntity(Loc.GetString("ratvar-servant-convert-success-others", ("target", victim)), victim, Filter.PvsExcept(victim), true, PopupType.MediumCaution);
    }

    /// <summary>
    /// Adds every servant action directly and records what was granted on the component itself, so
    /// DeconvertServant can revoke exactly this set later. Deliberately not using ActionGrant (whose
    /// MapInit-only trigger never fires for a component added to an already-live entity) - this is the
    /// single path for both round-start antags and Altar conversions, called from OnServantStartup.
    /// </summary>
    private void GrantServantActions(Entity<RatvarServantComponent> ent)
    {
        foreach (var action in ServantActions)
        {
            EntityUid? actionId = null;
            if (_actionsSystem.AddAction(ent.Owner, ref actionId, action) && actionId is { } id)
                ent.Comp.GrantedActions.Add(id);
        }
    }

    /// <summary>
    /// Everything a fresh Servant of Ratvar needs, applied uniformly whether they were granted
    /// RatvarServantComponent at round start or via Altar conversion:
    /// - Ratvar NPC faction membership, so summoned constructs don't attack their own servants
    ///   (NpcFactionSystem.GetNearbyHostiles ignores targets whose factions overlap the attacker's -
    ///   see ai_factions.yml).
    /// - Reactive to Holywater's "Unholy" tag (Touch), the holy-water counterplay hook - see
    ///   RatvarDeconvert/RatvarDeconvertEntityEffectSystem.
    /// - The slab/economy/craft actions (see GrantServantActions).
    /// </summary>
    private void OnServantStartup(Entity<RatvarServantComponent> ent, ref ComponentStartup args)
    {
        _npcFaction.AddFaction(ent.Owner, "Ratvar");

        var reactive = EnsureComp<ReactiveComponent>(ent.Owner);
        reactive.ReactiveGroups ??= new Dictionary<string, HashSet<ReactionMethod>>();
        if (!reactive.ReactiveGroups.TryGetValue("Unholy", out var methods))
        {
            methods = new HashSet<ReactionMethod>();
            reactive.ReactiveGroups["Unholy"] = methods;
        }
        methods.Add(ReactionMethod.Touch);

        GrantServantActions(ent);
    }

    /// <summary>
    /// Holy water counterplay landing point (see RatvarDeconvertEntityEffectSystem) - undoes everything
    /// OnServantStartup/GrantServantActions set up.
    /// </summary>
    public void DeconvertServant(EntityUid target)
    {
        if (!TryComp<RatvarServantComponent>(target, out var comp))
            return;

        foreach (var actionId in comp.GrantedActions)
        {
            _actionsSystem.RemoveAction(target, actionId);
        }

        _npcFaction.RemoveFaction(target, "Ratvar");

        if (TryComp<ReactiveComponent>(target, out var reactive))
            reactive.ReactiveGroups?.Remove("Unholy");

        if (_mind.TryGetMind(target, out var mindId, out _))
            _role.MindRemoveRole(mindId, "MindRoleRatvarServant");

        RemComp<RatvarServantComponent>(target);

        _popup.PopupEntity(Loc.GetString("ratvar-servant-deconverted"), target, PopupType.LargeCaution);
    }

    private EntityUid? FindOfferingVictim(Entity<RatvarAltarComponent> altar)
    {
        foreach (var candidate in _lookup.GetEntitiesInRange<MobStateComponent>(Transform(altar).Coordinates, 1f))
        {
            if (HasComp<RatvarServantComponent>(candidate) || !_mobState.IsIncapacitated(candidate))
                continue;

            if (!_mind.TryGetMind(candidate, out _, out _))
                continue;

            return candidate;
        }

        return null;
    }

    #endregion

    #region Clockwork Slab abilities

    private void OnSlabStun(Entity<RatvarServantComponent> ent, ref RatvarSlabStunActionEvent args)
    {
        if (args.Handled || !TryComp<MobStateComponent>(args.Target, out _))
            return;

        if (!_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 1.5f))
            return;

        args.Handled = true;

        _stun.TryAddParalyzeDuration(args.Target, TimeSpan.FromSeconds(4));
        EnsureComp<MutedComponent>(args.Target);
        var tempMute = EnsureComp<TemporaryMuteComponent>(args.Target);
        tempMute.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(8);

        _popup.PopupEntity(Loc.GetString("ratvar-servant-slab-stun-success", ("target", args.Target)), ent, PopupType.MediumCaution);
    }

    private void OnSlabTeleport(Entity<RatvarServantComponent> ent, ref RatvarSlabTeleportActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        _xform.SetCoordinates(ent.Owner, args.Target);
        _popup.PopupEntity(Loc.GetString("ratvar-servant-slab-teleport-success"), ent, PopupType.Medium);
    }

    private void OnSlabHeal(Entity<RatvarServantComponent> ent, ref RatvarSlabHealActionEvent args)
    {
        if (args.Handled || !HasComp<RatvarServantComponent>(args.Target))
            return;

        if (!_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 1.5f))
            return;

        args.Handled = true;

        _damageable.TryChangeDamage(args.Target, new DamageSpecifier(_proto.Index(BruteDamageType), -30f), ignoreResistances: true);
        _damageable.TryChangeDamage(args.Target, new DamageSpecifier(_proto.Index(BurnDamageType), -30f), ignoreResistances: true);

        _popup.PopupEntity(Loc.GetString("ratvar-servant-slab-heal-success", ("target", args.Target)), ent, PopupType.Medium);
    }

    #endregion

    #region Brass/energy economy

    /// <summary>
    /// Porting the ss220.space RU wiki's "Рука Мидаса": converts a touched stack of steel or plasteel
    /// into brass sheets, 1:1.
    /// </summary>
    private void OnHandOfMidas(Entity<RatvarServantComponent> ent, ref RatvarHandOfMidasActionEvent args)
    {
        if (args.Handled || !TryComp<StackComponent>(args.Target, out var stack))
            return;

        if (stack.StackTypeId != "Steel" && stack.StackTypeId != "Plasteel")
            return;

        if (!_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 1.5f))
            return;

        args.Handled = true;

        var count = _stackSystem.GetCount((args.Target, stack));
        var coords = Transform(args.Target).Coordinates;
        QueueDel(args.Target);
        _stackSpawn.SpawnAtPosition(count, BrassStackType, coords);

        _popup.PopupEntity(Loc.GetString("ratvar-servant-hand-of-midas-success"), ent, PopupType.Medium);
    }

    /// <summary>
    /// Porting the ss220.space RU wiki's beacon cost ("маяк - 6 листов латуни"): consumes brass from the
    /// servant's active hand and places a Herald's Beacon.
    /// </summary>
    private void OnPlaceBeacon(Entity<RatvarServantComponent> ent, ref RatvarPlaceBeaconActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryConsumeBrass(ent.Owner, BeaconBrassCost))
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-beacon-not-enough-brass", ("count", BeaconBrassCost)), ent, ent);
            return;
        }

        args.Handled = true;

        Spawn("RatvarHeraldsBeacon", args.Target);
        _popup.PopupEntity(Loc.GetString("ratvar-servant-beacon-placed"), ent, PopupType.Medium);
    }

    private void UpdateBeacons()
    {
        var beaconQuery = EntityQueryEnumerator<RatvarHeraldsBeaconComponent>();
        while (beaconQuery.MoveNext(out var beaconUid, out var beacon))
        {
            if (_timing.CurTime < beacon.NextTick)
                continue;

            beacon.NextTick = _timing.CurTime + beacon.TickInterval;

            var ruleQuery = EntityQueryEnumerator<RatvarServantRuleComponent>();
            while (ruleQuery.MoveNext(out _, out var ruleComp))
                ruleComp.Energy += beacon.EnergyPerTick;

            var coords = Transform(beaconUid).Coordinates;
            foreach (var servant in _lookup.GetEntitiesInRange<RatvarServantComponent>(coords, beacon.Range))
            {
                if (!_mobState.IsAlive(servant))
                    continue;

                _damageable.TryChangeDamage(servant.Owner, new DamageSpecifier(_proto.Index(BruteDamageType), -beacon.HealPerTick), ignoreResistances: true);
                _damageable.TryChangeDamage(servant.Owner, new DamageSpecifier(_proto.Index(BurnDamageType), -beacon.HealPerTick), ignoreResistances: true);
            }
        }
    }

    #endregion

    #region Craftable gear

    /// <summary>
    /// Reads cost/result data off the action entity itself (RatvarCraftGearComponent) - one handler
    /// covers every craftable item instead of a separate event/handler pair per item.
    /// </summary>
    private void OnCraftGear(Entity<RatvarServantComponent> ent, ref RatvarCraftGearActionEvent args)
    {
        if (args.Handled || !TryComp<RatvarCraftGearComponent>(args.Action, out var craft))
            return;

        if (!TryConsumeBrass(ent.Owner, craft.BrassCost))
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-craft-not-enough-brass", ("count", craft.BrassCost)), ent, ent);
            return;
        }

        if (!TryConsumeEnergy(craft.EnergyCost))
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-craft-not-enough-energy", ("count", craft.EnergyCost)), ent, ent);
            return;
        }

        args.Handled = true;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, craft.CraftTime, new RatvarCraftGearDoAfterEvent(), ent, used: args.Action)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnCraftGearDoAfter(Entity<RatvarServantComponent> ent, ref RatvarCraftGearDoAfterEvent args)
    {
        if (args.Cancelled || args.Args.Used is not { } actionEnt || !TryComp<RatvarCraftGearComponent>(actionEnt, out var craft))
            return;

        var item = Spawn(craft.ResultProto, Transform(ent).Coordinates);
        _hands.TryPickupAnyHand(ent, item);

        _popup.PopupEntity(Loc.GetString("ratvar-servant-craft-success"), ent, PopupType.Medium);
    }

    /// <summary>
    /// Animates a construct ally (Cogscarab/Marauder/Brass Golem). Shares RatvarCraftGearComponent
    /// for cost/result data with the item-crafting actions above, but is capped by
    /// RatvarServantRuleComponent.MaxConstructs instead of spawning into the servant's hands.
    /// </summary>
    private void OnCraftConstruct(Entity<RatvarServantComponent> ent, ref RatvarCraftConstructActionEvent args)
    {
        if (args.Handled || !TryComp<RatvarCraftGearComponent>(args.Action, out var craft))
            return;

        if (!TryGetServantRule(out var rule))
            return;

        PruneDeadConstructs(rule);

        if (rule.ActiveConstructs.Count >= RatvarServantRuleComponent.MaxConstructs)
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-construct-limit", ("count", RatvarServantRuleComponent.MaxConstructs)), ent, ent);
            return;
        }

        if (!TryConsumeBrass(ent.Owner, craft.BrassCost))
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-craft-not-enough-brass", ("count", craft.BrassCost)), ent, ent);
            return;
        }

        if (!TryConsumeEnergy(craft.EnergyCost))
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-craft-not-enough-energy", ("count", craft.EnergyCost)), ent, ent);
            return;
        }

        args.Handled = true;

        var doAfterArgs = new DoAfterArgs(EntityManager, ent, craft.CraftTime, new RatvarCraftConstructDoAfterEvent(), ent, used: args.Action)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnCraftConstructDoAfter(Entity<RatvarServantComponent> ent, ref RatvarCraftConstructDoAfterEvent args)
    {
        if (args.Cancelled || args.Args.Used is not { } actionEnt || !TryComp<RatvarCraftGearComponent>(actionEnt, out var craft))
            return;

        var construct = Spawn(craft.ResultProto, Transform(ent).Coordinates);

        if (TryGetServantRule(out var rule))
            rule.ActiveConstructs.Add(construct);

        _popup.PopupEntity(Loc.GetString("ratvar-servant-construct-summoned", ("name", Name(construct))), ent, PopupType.LargeCaution);
    }

    private bool TryGetServantRule(out RatvarServantRuleComponent rule)
    {
        var ruleQuery = EntityQueryEnumerator<RatvarServantRuleComponent>();
        if (ruleQuery.MoveNext(out _, out var comp))
        {
            rule = comp;
            return true;
        }

        rule = null!;
        return false;
    }

    private void PruneDeadConstructs(RatvarServantRuleComponent rule)
    {
        rule.ActiveConstructs.RemoveAll(uid => !Exists(uid) || !_mobState.IsAlive(uid));
    }

    /// <summary>
    /// Finds a Brass stack in the servant's active hand and removes <paramref name="amount"/> from it.
    /// Returns false (consuming nothing) if the active hand doesn't hold enough.
    /// </summary>
    private bool TryConsumeBrass(EntityUid user, int amount)
    {
        if (amount <= 0)
            return true;

        if (!_hands.TryGetActiveItem((user, null), out var heldItem) ||
            !TryComp<StackComponent>(heldItem, out var stack) ||
            stack.StackTypeId != BrassStackType)
        {
            return false;
        }

        var current = _stackSystem.GetCount((heldItem.Value, stack));
        if (current < amount)
            return false;

        _stackSystem.SetCount((heldItem.Value, stack), current - amount);
        return true;
    }

    private bool TryConsumeEnergy(float amount)
    {
        if (amount <= 0)
            return true;

        var ruleQuery = EntityQueryEnumerator<RatvarServantRuleComponent>();
        while (ruleQuery.MoveNext(out _, out var ruleComp))
        {
            if (ruleComp.Energy < amount)
                return false;

            ruleComp.Energy -= amount;
            return true;
        }

        return false;
    }

    #endregion

    #region Endgame ritual

    /// <summary>
    /// Porting the RU wiki's endgame chain: Workshop -> Strange Shard -> place on Altar -> defend 3
    /// minutes -> Ratvar summoned.
    /// </summary>
    private void OnWorkshopInteractHand(Entity<RatvarWorkshopComponent> ent, ref InteractHandEvent args)
    {
        if (!HasComp<RatvarServantComponent>(args.User))
            return;

        if (!TryConsumeBrass(args.User, ent.Comp.BrassCost))
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-craft-not-enough-brass", ("count", ent.Comp.BrassCost)), args.User, args.User);
            return;
        }

        if (!TryConsumeEnergy(ent.Comp.EnergyCost))
        {
            _popup.PopupClient(Loc.GetString("ratvar-servant-craft-not-enough-energy", ("count", ent.Comp.EnergyCost)), args.User, args.User);
            return;
        }

        var doAfterArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.CraftTime, new RatvarCraftShardDoAfterEvent(), ent.Owner, used: ent.Owner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = false,
        };

        _doAfter.TryStartDoAfter(doAfterArgs);
    }

    private void OnCraftShardDoAfter(Entity<RatvarWorkshopComponent> ent, ref RatvarCraftShardDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        var user = args.Args.User;
        var shard = Spawn("RatvarStrangeShard", Transform(ent).Coordinates);
        _hands.TryPickupAnyHand(user, shard);

        _popup.PopupEntity(Loc.GetString("ratvar-servant-shard-crafted"), ent, PopupType.LargeCaution);
    }

    private void OnAltarInteractUsing(Entity<RatvarAltarComponent> ent, ref InteractUsingEvent args)
    {
        if (ent.Comp.RitualActive || !HasComp<RatvarStrangeShardComponent>(args.Used) || !HasComp<RatvarServantComponent>(args.User))
            return;

        args.Handled = true;
        QueueDel(args.Used);

        ent.Comp.RitualActive = true;
        ent.Comp.RitualEndTime = _timing.CurTime + ent.Comp.RitualDuration;

        _popup.PopupEntity(Loc.GetString("ratvar-servant-ritual-started", ("count", ent.Comp.RequiredServants)), ent, PopupType.LargeCaution);
    }

    private void UpdateFinalRitual()
    {
        var altarQuery = EntityQueryEnumerator<RatvarAltarComponent>();
        while (altarQuery.MoveNext(out var altarUid, out var altar))
        {
            if (!altar.RitualActive)
                continue;

            var livingNearby = 0;
            foreach (var servant in _lookup.GetEntitiesInRange<RatvarServantComponent>(Transform(altarUid).Coordinates, altar.Range))
            {
                if (_mobState.IsAlive(servant))
                    livingNearby++;
            }

            if (livingNearby < altar.RequiredServants)
            {
                altar.RitualActive = false;
                _popup.PopupEntity(Loc.GetString("ratvar-servant-ritual-interrupted"), altarUid, PopupType.LargeCaution);
                continue;
            }

            if (_timing.CurTime < altar.RitualEndTime)
                continue;

            altar.RitualActive = false;
            Spawn("MobRatvarSpawn", Transform(altarUid).Coordinates);

            var ruleQuery = EntityQueryEnumerator<RatvarServantRuleComponent>();
            while (ruleQuery.MoveNext(out _, out var ruleComp))
                ruleComp.RatvarSummoned = true;
        }
    }

    #endregion

    #region Counterplay

    /// <summary>
    /// "Conceal Gears" (RU wiki): toggles RatvarConcealedComponent on a targeted structure. No cost -
    /// low-stakes utility, not a resource sink like the crafting actions above.
    /// </summary>
    private void OnConcealGears(Entity<RatvarServantComponent> ent, ref RatvarConcealGearsActionEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<RatvarAltarComponent>(args.Target) &&
            !HasComp<RatvarHeraldsBeaconComponent>(args.Target) &&
            !HasComp<RatvarWorkshopComponent>(args.Target))
        {
            return;
        }

        if (!_interaction.InRangeUnobstructed(ent.Owner, args.Target, range: 3f))
            return;

        args.Handled = true;

        if (RemComp<RatvarConcealedComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("ratvar-servant-conceal-revealed"), ent, ent);
            return;
        }

        AddComp<RatvarConcealedComponent>(args.Target);
        _popup.PopupEntity(Loc.GetString("ratvar-servant-conceal-hidden"), ent, ent);
    }

    #endregion

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        UpdateBeacons();
        UpdateFinalRitual();
    }
}
