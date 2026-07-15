using System.Numerics;
using Content.Server._Sunset.Homelander;
using Content.Shared._Sunset.TheBoys;
using Content.Shared._Sunset.TheBoys.Components;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.Mobs;
using Content.Shared.MouseRotator;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.TheBoys;

/// <summary>
/// Grants/reverts a temporary Compound V power for the three of Butcher's team who actually manifest
/// V-derived superpowers in the show - Butcher (laser eyes), Hughie (teleport), and Kimiko (super
/// strength) - plus strong passive regeneration matching Homelander's own, for all three. Frenchie and
/// Mother's Milk never had powers in the show, so they're not tagged and just fall through to
/// CompoundV's poison branch like anyone else outside the team (see reagents.yml). Each power
/// component's ComponentStartup/Shutdown pair applies and then exactly reverts its buffs, so it's safe
/// regardless of how many times someone re-doses.
/// </summary>
public sealed class TheBoysPowersSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly HomelanderSystem _homelander = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private const string KimikoDamageModifierSet = "TheBoysKimikoV";

    private static readonly FixedPoint2 KimikoMeleeBonus = FixedPoint2.New(20);

    /// <summary>
    /// Melee damage multiplier for Butcher and Hughie while V-powered - Kimiko already gets her own
    /// flat unarmed-punch bonus (KimikoMeleeBonus) as her canonical super strength, so she's excluded;
    /// this instead scales whatever weapon they're actually holding, via GetMeleeDamageEvent (which
    /// - per SharedMeleeWeaponSystem.GetDamage - is raised on both the weapon AND its wielder).
    /// </summary>
    private const float PoweredMeleeDamageMultiplier = 2f;

    /// <summary>
    /// Redosing V again this soon after the last dose that actually granted a power (i.e. two
    /// ComponentStartups back to back) is punished with a small burn of Cellular damage - abusing the
    /// serum has a real biological cost, it's not a free repeatable buff.
    /// </summary>
    private static readonly TimeSpan RedoseCooldown = TimeSpan.FromMinutes(1);

    private static readonly DamageSpecifier RedoseAbuseDamage = new()
    {
        DamageDict = new() { { "Cellular", 5 } },
    };

    private readonly Dictionary<EntityUid, TimeSpan> _lastPowerGrant = new();

    private const string ButcherLaserEyesAction = "ActionTheBoysButcherLaserEyes";
    private const string HughieBlinkAction = "ActionTheBoysHughieBlink";

    private static readonly SoundSpecifier LaserSound =
        new SoundPathSpecifier("/Audio/_Sunset/Homelander/laser_eye.ogg");

    /// <summary>How often (in seconds) the laser channel deals damage.</summary>
    private const float LaserTickInterval = 0.1f;

    /// <summary>How often (in seconds) the laser channel re-fires its visual bolt.</summary>
    private const float LaserVisualInterval = 0.1f;

    /// <summary>See HomelanderSystem.EyeForwardOffset's identical doc comment.</summary>
    private const float EyeForwardOffset = 0.2f;

    /// <summary>
    /// Passive regen granted to every powered team member while their power is active - matches
    /// Homelander's own PassiveDamage exactly (see Resources/Prototypes/_Sunset/Homelander/game_rule.yml).
    /// </summary>
    private static readonly DamageSpecifier PowerRegen = new()
    {
        DamageDict = new()
        {
            { "Blunt", -3 },
            { "Slash", -3 },
            { "Piercing", -3 },
            { "Heat", -3 },
            { "Shock", -3 },
            { "Cold", -3 },
            { "Caustic", -3 },
            { "Asphyxiation", -3 },
            { "Bloodloss", -3 },
        },
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TheBoysButcherPowerComponent, ComponentStartup>(OnButcherStartup);
        SubscribeLocalEvent<TheBoysButcherPowerComponent, ComponentShutdown>(OnButcherShutdown);
        SubscribeLocalEvent<TheBoysButcherLaserEyesEvent>(OnButcherLaserEyes);
        SubscribeLocalEvent<TheBoysButcherPowerComponent, GetMeleeDamageEvent>(OnPoweredMeleeDamage);

        SubscribeLocalEvent<TheBoysHughiePowerComponent, ComponentStartup>(OnHughieStartup);
        SubscribeLocalEvent<TheBoysHughiePowerComponent, ComponentShutdown>(OnHughieShutdown);
        SubscribeLocalEvent<TheBoysHughiePowerComponent, GetMeleeDamageEvent>(OnPoweredMeleeDamage);

        SubscribeLocalEvent<TheBoysKimikoPowerComponent, ComponentStartup>(OnKimikoStartup);
        SubscribeLocalEvent<TheBoysKimikoPowerComponent, ComponentShutdown>(OnKimikoShutdown);

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _lastPowerGrant.Clear();
    }

    /// <summary>
    /// Burns a little Cellular damage if this power is being (re)granted less than RedoseCooldown
    /// after the last time it was - i.e. shooting up again right after the last dose wore off, rather
    /// than pacing themselves.
    /// </summary>
    private void CheckRedoseAbuse(EntityUid uid)
    {
        var now = _timing.CurTime;

        if (_lastPowerGrant.TryGetValue(uid, out var last) && now - last < RedoseCooldown)
            _damageable.TryChangeDamage(uid, RedoseAbuseDamage, origin: uid);

        _lastPowerGrant[uid] = now;
    }

    private void GrantRegen(EntityUid uid)
    {
        var passive = EnsureComp<PassiveDamageComponent>(uid);
        passive.AllowedStates = new() { MobState.Alive, MobState.Critical };
        passive.DamageCap = 0;
        passive.Damage = PowerRegen;
    }

    /// <summary>
    /// Doubles whatever melee damage Butcher/Hughie deal for as long as their power is active, no
    /// matter what weapon (or bare fists) they're using - shared by both power components since the
    /// effect and multiplier are identical for each.
    /// </summary>
    private void OnPoweredMeleeDamage<T>(Entity<T> ent, ref GetMeleeDamageEvent args) where T : IComponent
    {
        args.Damage *= PoweredMeleeDamageMultiplier;
    }

    private void OnButcherStartup(Entity<TheBoysButcherPowerComponent> ent, ref ComponentStartup args)
    {
        CheckRedoseAbuse(ent.Owner);
        GrantRegen(ent.Owner);

        EntityUid? actionId = null;
        _actions.AddAction(ent.Owner, ref actionId, ButcherLaserEyesAction);
        ent.Comp.GrantedAction = actionId;
    }

    private void OnButcherShutdown(Entity<TheBoysButcherPowerComponent> ent, ref ComponentShutdown args)
    {
        RemComp<PassiveDamageComponent>(ent.Owner);

        // In case his power expires mid-channel - clean up exactly like EndButcherLaser, but skip
        // the cooldown (the action is about to be removed outright) and any Dirty/RemComp calls that
        // would no-op anyway once the component itself is gone.
        if (ent.Comp.LaserActive)
        {
            RemComp<MouseRotatorComponent>(ent.Owner);
            _audio.Stop(ent.Comp.LaserSoundEntity);
        }

        _actions.RemoveAction(ent.Comp.GrantedAction);
    }

    /// <summary>
    /// Starts/toggles-off Butcher's twin-laser channel - mirrors HomelanderSystem.OnHeatVision
    /// exactly (see its doc comment), just keyed off TheBoysButcherPowerComponent instead of
    /// HomelanderComponent so Butcher isn't mistaken for the real Homelander elsewhere (e.g. his own
    /// crowbar's bonus-damage check).
    /// </summary>
    private void OnButcherLaserEyes(TheBoysButcherLaserEyesEvent ev)
    {
        if (ev.Handled)
            return;

        ev.Handled = true;

        var uid = ev.Performer;
        if (!TryComp<TheBoysButcherPowerComponent>(uid, out var comp))
            return;

        if (comp.LaserActive)
        {
            EndButcherLaser(uid, comp);
            return;
        }

        comp.LaserActive = true;
        comp.LaserEndTime = _timing.CurTime + ev.Duration;
        comp.LaserTickAccumulator = 0f;
        comp.LaserVisualAccumulator = LaserVisualInterval;
        comp.LaserDamagePerSecond = ev.DamagePerSecond;
        comp.LaserRange = ev.Range;
        comp.LaserEyeOffset = ev.EyeOffset;
        comp.LaserLockout = ev.Lockout;
        comp.LaserActionEntity = ev.Action.Owner;

        var rotator = EnsureComp<MouseRotatorComponent>(uid);
        rotator.AngleTolerance = Angle.FromDegrees(1.0);
        Dirty(uid, rotator);

        comp.LaserSoundEntity = _audio.PlayPvs(LaserSound, uid, AudioParams.Default.WithLoop(true))?.Entity;
    }

    private void EndButcherLaser(EntityUid uid, TheBoysButcherPowerComponent comp)
    {
        comp.LaserActive = false;
        RemComp<MouseRotatorComponent>(uid);

        _audio.Stop(comp.LaserSoundEntity);
        comp.LaserSoundEntity = null;

        if (comp.LaserActionEntity is { } action)
            _actions.SetCooldown(action, comp.LaserLockout);
    }

    private void FireButcherLaserTick(EntityUid uid, TheBoysButcherPowerComponent comp, float dt, bool showVisual)
    {
        var gazerMap = _xform.GetMapCoordinates(uid);
        if (gazerMap.MapId == MapId.Nullspace)
            return;

        var facing = _xform.GetWorldRotation(uid);
        var direction = facing.ToWorldVec();
        var perpendicular = (facing + Angle.FromDegrees(90)).ToWorldVec();
        var sideOffset = perpendicular * comp.LaserEyeOffset;
        var forwardOffset = direction * EyeForwardOffset;
        var eyeCenter = gazerMap.Offset(forwardOffset);

        _homelander.FireBeam(uid, eyeCenter.Offset(sideOffset), direction, comp.LaserRange, comp.LaserDamagePerSecond, dt, showVisual);
        _homelander.FireBeam(uid, eyeCenter.Offset(-sideOffset), direction, comp.LaserRange, comp.LaserDamagePerSecond, dt, showVisual);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<TheBoysButcherPowerComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.LaserActive)
                continue;

            if (_timing.CurTime >= comp.LaserEndTime)
            {
                EndButcherLaser(uid, comp);
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

            FireButcherLaserTick(uid, comp, dt, showVisual);
        }
    }

    private void OnHughieStartup(Entity<TheBoysHughiePowerComponent> ent, ref ComponentStartup args)
    {
        CheckRedoseAbuse(ent.Owner);
        GrantRegen(ent.Owner);

        EntityUid? actionId = null;
        _actions.AddAction(ent.Owner, ref actionId, HughieBlinkAction);
        ent.Comp.GrantedAction = actionId;
    }

    private void OnHughieShutdown(Entity<TheBoysHughiePowerComponent> ent, ref ComponentShutdown args)
    {
        RemComp<PassiveDamageComponent>(ent.Owner);

        _actions.RemoveAction(ent.Comp.GrantedAction);
    }

    private void OnKimikoStartup(Entity<TheBoysKimikoPowerComponent> ent, ref ComponentStartup args)
    {
        CheckRedoseAbuse(ent.Owner);

        if (TryComp<DamageableComponent>(ent, out var damageable))
            ent.Comp.PreviousDamageModifierSet = damageable.DamageModifierSetId;

        _damageable.SetDamageModifierSetId(ent.Owner, KimikoDamageModifierSet);
        AdjustMeleeDamage(ent.Owner, KimikoMeleeBonus);

        GrantRegen(ent.Owner);
    }

    private void OnKimikoShutdown(Entity<TheBoysKimikoPowerComponent> ent, ref ComponentShutdown args)
    {
        _damageable.SetDamageModifierSetId(ent.Owner, ent.Comp.PreviousDamageModifierSet);
        AdjustMeleeDamage(ent.Owner, -KimikoMeleeBonus);
        RemComp<PassiveDamageComponent>(ent.Owner);
    }

    private void AdjustMeleeDamage(EntityUid uid, FixedPoint2 delta)
    {
        if (!TryComp<MeleeWeaponComponent>(uid, out var melee))
            return;

        melee.Damage.DamageDict.TryGetValue("Blunt", out var current);
        melee.Damage.DamageDict["Blunt"] = current + delta;
        Dirty(uid, melee);
    }
}
