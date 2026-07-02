using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Inventory;
using Content.Shared.Maps;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Tag;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared._Sunset.Grab.Components;
using Content.Shared._Sunset.Grab.Events;
using Content.Shared._Sunset.MartialArts.Components;
using Content.Shared._Sunset.MartialArts.Events;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._Sunset.MartialArts.Systems;

/// <summary>
/// Core martial arts framework: granting/removing a style, and detecting combo sequences (Harm/
/// Disarm/Grab) to trigger each style's special moves. Style-specific move implementations live in
/// the SharedMartialArtsSystem.&lt;Style&gt;.cs partial files.
/// </summary>
public sealed partial class SharedMartialArtsSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private SharedActionsSystem _actions = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedStaminaSystem _stamina = default!;
    [Dependency] private SharedStunSystem _stun = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private ThrowingSystem _throwing = default!;
    [Dependency] private SharedTransformSystem _transform = default!;
    [Dependency] private EntityLookupSystem _lookup = default!;
    [Dependency] private AlertsSystem _alerts = default!;
    [Dependency] private PullingSystem _pulling = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private TagSystem _tag = default!;
    [Dependency] private InventorySystem _inventory = default!;
    [Dependency] private TurfSystem _turf = default!;
    [Dependency] private SharedMapSystem _mapSystem = default!;

    private const float CqcUnblockRange = 8f;

    // Agent CQC and Cook CQC share the exact same moveset - they only differ in where they're
    // allowed to be performed (see PerformCombo's kitchen-proximity check for MartialArtStyle.CqcCook).
    private static readonly List<(string Id, ComboAttackType[] Sequence, bool PerformOnSelf)> CqcCombos = new()
    {
        ("CqcConsecutive", new[] { ComboAttackType.Disarm, ComboAttackType.Disarm, ComboAttackType.Harm }, false),
        ("CqcPressure", new[] { ComboAttackType.Disarm, ComboAttackType.Grab }, false),
        ("CqcSlam", new[] { ComboAttackType.Grab, ComboAttackType.Harm }, false),
        ("CqcRestrain", new[] { ComboAttackType.Grab, ComboAttackType.Grab }, false),
        ("CqcKick", new[] { ComboAttackType.Harm, ComboAttackType.Harm }, false),
    };

    // Order within a style matters: longer sequences must be listed before any shorter sequence that
    // is one of their prefixes/suffixes, otherwise the shorter one would always fire first.
    private static readonly Dictionary<MartialArtStyle, List<(string Id, ComboAttackType[] Sequence, bool PerformOnSelf)>> Combos = new()
    {
        [MartialArtStyle.Ninjutsu] = new()
        {
            ("NinjutsuBiteTheDust", new[] { ComboAttackType.Harm, ComboAttackType.Grab }, false),
            ("NinjutsuDirtyKill", new[] { ComboAttackType.Disarm, ComboAttackType.Disarm }, false),
        },
        [MartialArtStyle.CQC] = CqcCombos,
        [MartialArtStyle.CqcCook] = CqcCombos,
        [MartialArtStyle.SleepingCarp] = new()
        {
            ("CarpKneeHaul", new[] { ComboAttackType.Harm, ComboAttackType.Grab }, false),
            ("CarpCrashingWaves", new[] { ComboAttackType.Harm, ComboAttackType.Disarm }, false),
            ("CarpGnashingTeeth", new[] { ComboAttackType.Harm, ComboAttackType.Harm }, false),
        },
        [MartialArtStyle.Capoeira] = new()
        {
            ("CapoeiraSpinKick", new[] { ComboAttackType.Grab, ComboAttackType.Harm, ComboAttackType.Disarm, ComboAttackType.Harm }, false),
            ("CapoeiraPushKick", new[] { ComboAttackType.Grab, ComboAttackType.Harm, ComboAttackType.Harm }, false),
            ("CapoeiraCircleKick", new[] { ComboAttackType.Disarm, ComboAttackType.Disarm, ComboAttackType.Harm }, false),
            ("CapoeiraSweepKick", new[] { ComboAttackType.Harm, ComboAttackType.Harm, ComboAttackType.Disarm }, false),
            ("CapoeiraKickUp", new[] { ComboAttackType.Disarm, ComboAttackType.Disarm }, true),
        },
        [MartialArtStyle.KungFuDragon] = new()
        {
            ("DragonTail", new[] { ComboAttackType.Disarm, ComboAttackType.Grab, ComboAttackType.Harm }, false),
            ("DragonStrike", new[] { ComboAttackType.Disarm, ComboAttackType.Harm, ComboAttackType.Harm }, false),
            ("DragonClaw", new[] { ComboAttackType.Disarm, ComboAttackType.Disarm }, false),
        },
        [MartialArtStyle.CorporateJudo] = new()
        {
            ("JudoArmbar", new[] { ComboAttackType.Disarm, ComboAttackType.Disarm, ComboAttackType.Grab }, false),
            ("JudoWheelThrow", new[] { ComboAttackType.Grab, ComboAttackType.Disarm, ComboAttackType.Harm }, false),
            ("JudoThrow", new[] { ComboAttackType.Grab, ComboAttackType.Disarm }, false),
            ("JudoDiscombobulate", new[] { ComboAttackType.Disarm, ComboAttackType.Grab }, false),
            ("JudoEyePoke", new[] { ComboAttackType.Disarm, ComboAttackType.Harm }, false),
        },
        [MartialArtStyle.Mime] = new()
        {
            ("MimeFingerGuns", new[] { ComboAttackType.Harm, ComboAttackType.Harm }, false),
            ("MimeBoxTrap", new[] { ComboAttackType.Grab, ComboAttackType.Disarm }, false),
            ("MimeExaggeratedSlap", new[] { ComboAttackType.Harm, ComboAttackType.Disarm }, false),
        },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MartialArtManualComponent, UseInHandEvent>(OnManualUsed);

        SubscribeLocalEvent<MartialArtsKnowledgeComponent, MeleeHitEvent>(OnMeleeHit);
        SubscribeLocalEvent<MeleeDisarmAttemptedEvent>(OnDisarmAttempted);
        SubscribeLocalEvent<MartialArtsKnowledgeComponent, GrabEscalatedEvent>(OnGrabEscalated);

        SubscribeLocalEvent<MartialArtsKnowledgeComponent, ComponentShutdown>(OnKnowledgeShutdown);

        // Single broadcast subscription for the whole system - Sleeping Carp's no-guns rule and the
        // Corporate Judo belt's stunner-only rule both hook this event, and Robust doesn't allow the same
        // system instance to subscribe to the same broadcast event type twice.
        SubscribeLocalEvent<ShotAttemptedEvent>(OnShotAttempted);

        InitializeNinjutsu();
        InitializeSleepingCarp();
        InitializeCapoeira();
        InitializeKungFuDragon();
        InitializeCorporateJudo();
        InitializeMime();
    }

    private void OnShotAttempted(ref ShotAttemptedEvent args)
    {
        OnCarpShotAttempt(ref args);
        OnJudoBeltShotAttempt(ref args);
    }

    #region Granting

    public bool TryGrantMartialArt(EntityUid uid, MartialArtStyle style)
    {
        if (style == MartialArtStyle.None)
            return false;

        if (TryComp<MartialArtsKnowledgeComponent>(uid, out var existingKnowledge) && existingKnowledge.Style != MartialArtStyle.None)
            return false;

        var knowledge = EnsureComp<MartialArtsKnowledgeComponent>(uid);
        knowledge.Style = style;

        if (TryComp<MeleeWeaponComponent>(uid, out var melee))
        {
            knowledge.OriginalFistDamage = melee.Damage;
            if (GetStyleFistDamage(style) is { } styleDamage)
                melee.Damage = styleDamage;
            Dirty(uid, melee);
        }

        Dirty(uid, knowledge);
        EnsureComp<CanPerformComboComponent>(uid);
        GrantStyleComponents(uid, style);

        return true;
    }

    private void OnKnowledgeShutdown(Entity<MartialArtsKnowledgeComponent> ent, ref ComponentShutdown args)
    {
        // If the whole entity is being deleted, EntityManager is already tearing down every
        // component on it - manually RemComp-ing here would hit components mid-deletion and log
        // "deleting an already deleted component" spam.
        if (TerminatingOrDeleted(ent.Owner))
            return;

        if (ent.Comp.OriginalFistDamage is { } original && TryComp<MeleeWeaponComponent>(ent.Owner, out var melee))
        {
            melee.Damage = original;
            Dirty(ent.Owner, melee);
        }

        if (HasComp<CanPerformComboComponent>(ent.Owner))
            RemComp<CanPerformComboComponent>(ent.Owner);

        RevokeStyleComponents(ent.Owner, ent.Comp.Style);
    }

    private static DamageSpecifier? GetStyleFistDamage(MartialArtStyle style) => style switch
    {
        MartialArtStyle.SleepingCarp => new DamageSpecifier { DamageDict = new() { { "Slash", 5 } } },
        _ => null,
    };

    private void GrantStyleComponents(EntityUid uid, MartialArtStyle style)
    {
        switch (style)
        {
            case MartialArtStyle.Ninjutsu:
                EnsureComp<NinjutsuSneakAttackComponent>(uid);
                break;
            case MartialArtStyle.SleepingCarp:
                // Mastery progress (SleepingCarpMasteryComponent) is already tracked via re-using the
                // scroll by the time this runs - see OnSleepingCarpScrollUsed.
                break;
            case MartialArtStyle.Capoeira:
                EnsureComp<CapoeiraMomentumComponent>(uid);
                break;
            case MartialArtStyle.KungFuDragon:
                EnsureComp<DragonPowerComponent>(uid);
                break;
            case MartialArtStyle.Mime:
                var mimery = EnsureComp<MimeAdvancedMimeryComponent>(uid);
                _actions.AddAction(uid, ref mimery.BlockadeAction, "ActionMartialArtsMimeInvisibleBlockade");
                break;
        }
    }

    private void RevokeStyleComponents(EntityUid uid, MartialArtStyle style)
    {
        switch (style)
        {
            case MartialArtStyle.Ninjutsu:
                if (HasComp<NinjutsuSneakAttackComponent>(uid))
                    RemComp<NinjutsuSneakAttackComponent>(uid);
                break;
            case MartialArtStyle.SleepingCarp:
                if (HasComp<SleepingCarpMasteryComponent>(uid))
                    RemComp<SleepingCarpMasteryComponent>(uid);
                break;
            case MartialArtStyle.Capoeira:
                if (HasComp<CapoeiraMomentumComponent>(uid))
                    RemComp<CapoeiraMomentumComponent>(uid);
                break;
            case MartialArtStyle.KungFuDragon:
                if (HasComp<DragonPowerComponent>(uid))
                    RemComp<DragonPowerComponent>(uid);
                break;
            case MartialArtStyle.Mime:
                if (TryComp<MimeAdvancedMimeryComponent>(uid, out var mimery))
                {
                    _actions.RemoveAction(mimery.BlockadeAction);
                    RemComp<MimeAdvancedMimeryComponent>(uid);
                }
                break;
        }
    }

    private void OnManualUsed(Entity<MartialArtManualComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        // Sleeping Carp's scroll has its own multi-use training ritual instead of a single-use grant.
        if (ent.Comp.Style == MartialArtStyle.SleepingCarp)
        {
            OnSleepingCarpScrollUsed(ent, ref args);
            return;
        }

        if (!TryGrantMartialArt(args.User, ent.Comp.Style))
        {
            _popup.PopupClient(Loc.GetString("martial-arts-already-known"), args.User, args.User);
            return;
        }

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("martial-arts-learned", ("style", GetStyleLocName(ent.Comp.Style))), args.User, args.User, PopupType.Medium);
        PredictedQueueDel(ent.Owner);
    }

    public string GetStyleLocName(MartialArtStyle style) => style switch
    {
        MartialArtStyle.Ninjutsu => Loc.GetString("martial-arts-style-ninjutsu"),
        MartialArtStyle.CQC => Loc.GetString("martial-arts-style-cqc"),
        MartialArtStyle.CqcCook => Loc.GetString("martial-arts-style-cqc-cook"),
        MartialArtStyle.SleepingCarp => Loc.GetString("martial-arts-style-sleeping-carp"),
        MartialArtStyle.Capoeira => Loc.GetString("martial-arts-style-capoeira"),
        MartialArtStyle.KungFuDragon => Loc.GetString("martial-arts-style-kungfu-dragon"),
        MartialArtStyle.CorporateJudo => Loc.GetString("martial-arts-style-corporate-judo"),
        MartialArtStyle.Mime => Loc.GetString("martial-arts-style-mime"),
        _ => string.Empty,
    };

    #endregion

    #region Combo detection

    private void OnMeleeHit(Entity<MartialArtsKnowledgeComponent> ent, ref MeleeHitEvent args)
    {
        // Only unarmed strikes count towards combos - the "weapon" must be the practitioner's own body.
        if (args.Weapon != ent.Owner)
            return;

        if (args.HitEntities.Count == 0)
        {
            OnMiss(ent);
            return;
        }

        RecordAttack(ent.Owner, args.HitEntities[0], ComboAttackType.Harm);
    }

    private void OnDisarmAttempted(ref MeleeDisarmAttemptedEvent args)
    {
        // Recorded on attempt (like Goob Station), not on success - a shove/disarm still "counts"
        // towards a combo even if the target resists it.
        RecordAttack(args.User, args.Target, ComboAttackType.Disarm);
    }

    private void OnGrabEscalated(Entity<MartialArtsKnowledgeComponent> ent, ref GrabEscalatedEvent args)
    {
        RecordAttack(ent.Owner, args.Grabbed, ComboAttackType.Grab);
    }

    private void RecordAttack(EntityUid user, EntityUid target, ComboAttackType type)
    {
        if (!TryComp<MartialArtsKnowledgeComponent>(user, out var knowledge) || knowledge.Style == MartialArtStyle.None)
            return;

        if (!TryComp<CanPerformComboComponent>(user, out var combo))
            return;

        var curTime = _timing.CurTime;

        if (combo.CurrentTarget != target || curTime - combo.LastAttackTime > combo.ResetWindow)
            combo.LastAttacks.Clear();

        combo.CurrentTarget = target;
        combo.LastAttackTime = curTime;
        combo.LastAttacks.Add(type);

        if (combo.LastAttacks.Count > combo.LastAttacksLimit)
            combo.LastAttacks.RemoveAt(0);

        Dirty(user, combo);

        // No popup here on purpose - the combo tracker HUD (ComboTrackerOverlay) already shows this
        // visually next to the cursor, a chat message on every single hit would just be spam.
        TryMatchCombo(user, target, knowledge.Style, combo);
    }

    private void TryMatchCombo(EntityUid user, EntityUid target, MartialArtStyle style, CanPerformComboComponent combo)
    {
        if (!Combos.TryGetValue(style, out var combos))
            return;

        foreach (var (id, sequence, performOnSelf) in combos)
        {
            if (performOnSelf != (target == user))
                continue;

            if (sequence.Length > combo.LastAttacks.Count)
                continue;

            var tail = combo.LastAttacks.Skip(combo.LastAttacks.Count - sequence.Length);
            if (!tail.SequenceEqual(sequence))
                continue;

            combo.LastAttacks.Clear();
            Dirty(user, combo);
            PerformCombo(id, user, target);
            return;
        }
    }

    /// <summary>
    /// Cook CQC is identical to Agent CQC, but - unlike the version sold in the syndicate uplink - it
    /// only works while near a kitchen (a <c>SpawnPointChef</c> marker), checked on every attempt.
    /// </summary>
    private bool IsNearKitchen(EntityUid user)
    {
        foreach (var nearby in _lookup.GetEntitiesInRange(user, CqcUnblockRange))
        {
            if (MetaData(nearby).EntityPrototype?.ID == "SpawnPointChef")
                return true;
        }

        return false;
    }

    private void PerformCombo(string comboId, EntityUid user, EntityUid target)
    {
        if (TryComp<MartialArtsKnowledgeComponent>(user, out var knowledge) &&
            knowledge.Style == MartialArtStyle.CqcCook && !IsNearKitchen(user))
        {
            _popup.PopupClient(Loc.GetString("martial-arts-cqc-blocked"), user, user);
            return;
        }

        switch (comboId)
        {
            case "NinjutsuBiteTheDust": BiteTheDust(user, target); break;
            case "NinjutsuDirtyKill": DirtyKill(user, target); break;
            case "CqcSlam": CqcSlam(user, target); break;
            case "CqcKick": CqcKick(user, target); break;
            case "CqcRestrain": CqcRestrain(user, target); break;
            case "CqcPressure": CqcPressure(user, target); break;
            case "CqcConsecutive": CqcConsecutive(user, target); break;
            case "CarpGnashingTeeth": CarpGnashingTeeth(user, target); break;
            case "CarpKneeHaul": CarpKneeHaul(user, target); break;
            case "CarpCrashingWaves": CarpCrashingWaves(user, target); break;
            case "CapoeiraPushKick": CapoeiraPushKick(user, target); break;
            case "CapoeiraCircleKick": CapoeiraCircleKick(user, target); break;
            case "CapoeiraSweepKick": CapoeiraSweepKick(user, target); break;
            case "CapoeiraSpinKick": CapoeiraSpinKick(user, target); break;
            case "CapoeiraKickUp": CapoeiraKickUp(user); break;
            case "DragonClaw": DragonClaw(user, target); break;
            case "DragonTail": DragonTail(user, target); break;
            case "DragonStrike": DragonStrike(user, target); break;
            case "JudoDiscombobulate": JudoDiscombobulate(user, target); break;
            case "JudoEyePoke": JudoEyePoke(user, target); break;
            case "JudoThrow": JudoThrow(user, target); break;
            case "JudoArmbar": JudoArmbar(user, target); break;
            case "JudoWheelThrow": JudoWheelThrow(user, target); break;
            case "MimeFingerGuns": MimeFingerGuns(user, target); break;
            case "MimeBoxTrap": MimeBoxTrap(user, target); break;
            case "MimeExaggeratedSlap": MimeExaggeratedSlap(user, target); break;
        }
    }

    private void OnMiss(Entity<MartialArtsKnowledgeComponent> ent)
    {
        if (ent.Comp.Style == MartialArtStyle.Capoeira)
            CapoeiraOnMiss(ent.Owner);
    }

    #endregion

    #region Shared helpers

    private void ThrowAt(EntityUid thrower, EntityUid target, float speed)
    {
        var direction = _transform.GetMapCoordinates(target).Position - _transform.GetMapCoordinates(thrower).Position;
        if (direction.LengthSquared() < 0.01f)
            return;

        _throwing.TryThrow(target, direction.Normalized(), speed, thrower);
    }

    #endregion

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        var curTime = _timing.CurTime;

        UpdateKungFuDragon(curTime);

        var sneakQuery = EntityQueryEnumerator<NinjutsuSneakAttackComponent>();
        while (sneakQuery.MoveNext(out var uid, out var sneak))
        {
            if (sneak.Revealed && curTime >= sneak.RevealedUntil)
            {
                sneak.Revealed = false;
                Dirty(uid, sneak);
                UpdateNinjutsuSneakAlert((uid, sneak));
            }
        }

        var muteQuery = EntityQueryEnumerator<TemporaryMuteComponent>();
        while (muteQuery.MoveNext(out var uid, out var mute))
        {
            if (curTime < mute.ExpiresAt)
                continue;

            RemComp<TemporaryMuteComponent>(uid);
            RemComp<Content.Shared.Speech.Muting.MutedComponent>(uid);
        }
    }
}
