using System.Linq;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Alert.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Shared._Sunset.Sandevistan;

/// <summary>
/// <see cref="SandevistanUserComponent"/> - toggle, load-meter ticking/threshold effects, movement/
/// attack-speed boost, and the mob-only slowfield. See SandevistanUserComponent.cs for what's been
/// trimmed from the original Reserve-Station/Goobstation version.
/// </summary>
public sealed class SandevistanSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SandevistanUserComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<SandevistanUserComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SandevistanUserComponent, ToggleSandevistanEvent>(OnToggle);
        SubscribeLocalEvent<SandevistanUserComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<SandevistanUserComponent, MeleeAttackEvent>(OnMeleeAttack);
        SubscribeLocalEvent<SandevistanUserComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<SandevistanUserComponent, GetGenericAlertCounterAmountEvent>(OnGetCounterAmount);

        SubscribeLocalEvent<SandevistanSlowedComponent, RemoveSandevistanSlowdownEvent>(OnRemoveSlowdown);
        SubscribeLocalEvent<SandevistanSlowedComponent, RefreshMovementSpeedModifiersEvent>(OnSlowedRefreshSpeed);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var cleanupQuery = EntityQueryEnumerator<SandevistanSlowedComponent>();
        while (cleanupQuery.MoveNext(out var target, out var slowed))
        {
            if (!slowed.IsSlowed)
                RemComp(target, slowed);
        }

        if (_netManager.IsServer)
        {
            var glitchQuery = EntityQueryEnumerator<SandevistanGlitchComponent>();
            while (glitchQuery.MoveNext(out var glitchUid, out var glitchComp))
            {
                if (_timing.CurTime >= glitchComp.ExpiresAt)
                    RemCompDeferred<SandevistanGlitchComponent>(glitchUid);
            }

            var inactiveQuery = EntityQueryEnumerator<SandevistanUserComponent>();
            while (inactiveQuery.MoveNext(out var inactiveUid, out var inactiveComp))
            {
                if (inactiveComp.Active || inactiveComp.CurrentLoad <= 0f)
                    continue;

                inactiveComp.CurrentLoad = MathF.Max(0f, inactiveComp.CurrentLoad + inactiveComp.LoadPerInactiveSecond * frameTime);
                Dirty(inactiveUid, inactiveComp);
            }
        }

        var query = EntityQueryEnumerator<ActiveSandevistanUserComponent, SandevistanUserComponent>();
        while (query.MoveNext(out var uid, out _, out var comp))
        {
            UpdateAfterimages(uid, comp);

            if (comp.SlowfieldEnabled && _netManager.IsServer)
                UpdateSlowfield(uid, comp);

            if (_netManager.IsServer)
            {
                comp.CurrentLoad += comp.LoadPerActiveSecond * frameTime;
                Dirty(uid, comp);
            }

            var filteredStates = new List<int>();
            foreach (var stateThreshold in comp.Thresholds)
                if (comp.CurrentLoad >= stateThreshold.Value)
                    filteredStates.Add((int) stateThreshold.Key);

            filteredStates.Sort((a, b) => b.CompareTo(a));
            foreach (var state in filteredStates)
            {
                if (!comp.Effects.TryGetValue((SandevistanState) state, out var effects))
                    continue;

                foreach (var effect in effects)
                    effect.Effect(uid, comp, EntityManager, frameTime);
            }

            if (comp.NextPopupTime > _timing.CurTime)
            {
                Dirty(uid, comp);
                continue;
            }

            var popup = -1;
            foreach (var state in filteredStates)
                if (state > popup && state < 4)
                    popup = state;

            if (popup == -1)
                continue;

            if (_netManager.IsServer)
                _popup.PopupEntity(Loc.GetString("sandevistan-overload-" + popup), uid, uid);

            comp.NextPopupTime = _timing.CurTime + comp.PopupDelay;
            Dirty(uid, comp);
        }
    }

    private void OnInit(Entity<SandevistanUserComponent> ent, ref ComponentInit args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ToggleActionEntity, ent.Comp.ToggleAction);
        _alerts.ShowAlert(ent.Owner, ent.Comp.LoadAlert);
        Dirty(ent);
    }

    private void OnToggle(Entity<SandevistanUserComponent> ent, ref ToggleSandevistanEvent args)
    {
        args.Handled = true;

        if (ent.Comp.Active)
        {
            _audio.PlayPredicted(ent.Comp.EndSound, ent, ent);
            Disable(ent, ent.Comp);
            Dirty(ent);
            return;
        }

        ent.Comp.Active = true;
        EnsureComp<ActiveSandevistanUserComponent>(ent);

        if (TryComp<SandevistanSlowedComponent>(ent, out var slowed))
        {
            var ev = new RemoveSandevistanSlowdownEvent(slowed.Source);
            RaiseLocalEvent(ent, ref ev);
        }

        _speed.RefreshMovementSpeedModifiers(ent);

        _audio.PlayPredicted(ent.Comp.StartSound, ent, ent);
        Dirty(ent);
        PlayLoopedAudio(ent, ent.Comp);
    }

    private void OnRefreshSpeed(Entity<SandevistanUserComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Active)
            args.ModifySpeed(ent.Comp.MovementSpeedModifier, ent.Comp.MovementSpeedModifier);
    }

    private void OnMeleeAttack(Entity<SandevistanUserComponent> ent, ref MeleeAttackEvent args)
    {
        if (!ent.Comp.Active || !TryComp<MeleeWeaponComponent>(args.Weapon, out var weapon))
            return;

        var rate = weapon.NextAttack - _timing.CurTime;
        weapon.NextAttack -= rate - rate / ent.Comp.AttackSpeedModifier;
    }

    private void OnMobStateChanged(Entity<SandevistanUserComponent> ent, ref MobStateChangedEvent args) =>
        Disable(ent, ent.Comp);

    private void OnGetCounterAmount(Entity<SandevistanUserComponent> ent, ref GetGenericAlertCounterAmountEvent args)
    {
        if (args.Handled || ent.Comp.LoadAlert != args.Alert)
            return;

        args.Amount = (int) ent.Comp.CurrentLoad;
    }

    private void OnShutdown(Entity<SandevistanUserComponent> ent, ref ComponentShutdown args)
    {
        Disable(ent, ent.Comp);
        _actions.RemoveAction(ent.Owner, ent.Comp.ToggleActionEntity);
        _alerts.ClearAlert(ent.Owner, ent.Comp.LoadAlert);
    }

    public void Disable(EntityUid uid, SandevistanUserComponent comp)
    {
        var wasActive = comp.Active;
        if (comp.Active)
        {
            if (comp.SlowfieldEnabled)
            {
                var query = EntityQueryEnumerator<SandevistanSlowedComponent>();
                while (query.MoveNext(out var target, out var slowed))
                {
                    if (slowed.Source != uid)
                        continue;

                    var ev = new RemoveSandevistanSlowdownEvent(uid);
                    RaiseLocalEvent(target, ref ev);
                }
            }

            RemCompDeferred<ActiveSandevistanUserComponent>(uid);
            comp.Active = false;
        }

        _speed.RefreshMovementSpeedModifiers(uid);
        DeleteAfterimages(uid);
        StopLoopedAudio(comp);

        if (wasActive)
            Dirty(uid, comp);
    }

    #region Afterimages

    /// <summary>Spawns a new rainbow-tinted afterimage every AfterimageInterval while active.</summary>
    public void UpdateAfterimages(EntityUid uid, SandevistanUserComponent comp)
    {
        if (_timing.CurTime >= comp.NextAfterimageTime)
        {
            SpawnAfterimage(uid, comp);
            comp.NextAfterimageTime = _timing.CurTime + TimeSpan.FromSeconds(comp.AfterimageInterval);
        }

        comp.ColorAccumulator++;
    }

    private void SpawnAfterimage(EntityUid uid, SandevistanUserComponent comp)
    {
        var xform = Transform(uid);
        var coordinates = xform.Coordinates;
        var afterimage = Spawn(null, coordinates);

        var afterimageComp = EnsureComp<SandevistanAfterimageComponent>(afterimage);
        afterimageComp.SourceEntity = uid;
        afterimageComp.Hue = comp.ColorAccumulator % 100f / 100f;
        afterimageComp.DirectionOverride = xform.LocalRotation.GetCardinalDir();
        Dirty(afterimage, afterimageComp);
    }

    /// <summary>Fades out (rather than instantly deleting) any afterimages left over once disabled.</summary>
    public void DeleteAfterimages(EntityUid sourceUid)
    {
        Timer.Spawn(TimeSpan.FromSeconds(1), () =>
        {
            var query = EntityQueryEnumerator<SandevistanAfterimageComponent>();
            while (query.MoveNext(out var afterimageUid, out var afterimageComp))
            {
                if (afterimageComp.SourceEntity != sourceUid)
                    continue;

                var despawn = EnsureComp<TimedDespawnComponent>(afterimageUid);
                despawn.Lifetime = 3f;
            }
        });
    }

    #endregion

    #region Audio

    public void PlayLoopedAudio(EntityUid uid, SandevistanUserComponent comp)
    {
        if (!_netManager.IsServer || comp.LoopSound == null || comp.PlayingStream != null)
            return;

        Timer.Spawn(TimeSpan.FromSeconds(comp.LoopSoundDelay), () =>
        {
            if (!Deleted(uid) && comp.Active && comp.PlayingStream == null)
            {
                var stream = _audio.PlayPvs(comp.LoopSound, uid);
                if (stream?.Entity is { } entity)
                    comp.PlayingStream = entity;
            }
        });
    }

    private void StopLoopedAudio(SandevistanUserComponent comp)
    {
        if (comp.PlayingStream != null)
        {
            _audio.Stop(comp.PlayingStream);
            comp.PlayingStream = null;
        }
    }

    #endregion

    #region Slowfield (mobs only)

    /// <summary>
    /// A periodic proximity check (not a physics sensor fixture) - deliberately avoids collision
    /// layers/masks entirely. Those are shared/overloaded across many unrelated physics concerns
    /// (blocking movement, blocking bullets, blocking line-of-sight) - a "detect nearby mobs" mask
    /// invariably ends up overlapping with structures that share one of those flags for an unrelated
    /// reason (e.g. grilles are BulletImpassable, same as mobs, for their own reasons), which is what
    /// caused the reported bug of the field reacting to grilles (and their electrification) from
    /// across the room. A plain radius query against MobStateComponent can't have that problem.
    /// </summary>
    private void UpdateSlowfield(EntityUid uid, SandevistanUserComponent comp)
    {
        var coords = Transform(uid).Coordinates;
        var nearby = _lookup.GetEntitiesInRange<MobStateComponent>(coords, comp.SlowfieldRadius);

        foreach (var mob in nearby)
        {
            if (mob.Owner == uid)
                continue;

            ApplySlowdown(uid, mob.Owner, comp);
        }

        var query = EntityQueryEnumerator<SandevistanSlowedComponent>();
        while (query.MoveNext(out var target, out var slowed))
        {
            if (slowed.Source != uid || !slowed.IsSlowed)
                continue;

            if (nearby.Any(e => e.Owner == target))
                continue;

            var ev = new RemoveSandevistanSlowdownEvent(uid);
            RaiseLocalEvent(target, ref ev);
        }
    }

    private void ApplySlowdown(EntityUid source, EntityUid target, SandevistanUserComponent comp)
    {
        if (TryComp<SandevistanSlowedComponent>(target, out var existing) && existing.IsSlowed)
            return;

        if (HasComp<ActiveSandevistanUserComponent>(target))
            return;

        var slowed = EnsureComp<SandevistanSlowedComponent>(target);
        slowed.IsSlowed = true;
        slowed.Source = source;
        slowed.SpeedMultiplier = comp.MobSpeedMultiplier;
        _speed.RefreshMovementSpeedModifiers(target);

        Dirty(target, slowed);
    }

    private void OnRemoveSlowdown(Entity<SandevistanSlowedComponent> ent, ref RemoveSandevistanSlowdownEvent args)
    {
        if (ent.Comp.Source != args.Source || !ent.Comp.IsSlowed)
            return;

        ent.Comp.IsSlowed = false;
        _speed.RefreshMovementSpeedModifiers(ent);
    }

    private void OnSlowedRefreshSpeed(Entity<SandevistanSlowedComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.IsSlowed)
            args.ModifySpeed(ent.Comp.SpeedMultiplier, ent.Comp.SpeedMultiplier);
    }

    #endregion
}
