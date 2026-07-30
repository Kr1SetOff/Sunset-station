using System.Linq;
using Content.Shared._Goobstation.SpecialPassives.Fleshmend.Components;
using Content.Shared._Starlight.Medical.Body.Systems;
using Content.Shared.Alert;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.SpecialPassives.Fleshmend;

public sealed class SharedFleshmendSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _netManager = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodstream = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;

    private EntityQuery<DamageableComponent> _damageableQuery;
    private EntityQuery<MobStateComponent> _mobstateQuery;
    private EntityQuery<FlammableComponent> _flammableQuery;

    public readonly ProtoId<DamageGroupPrototype> BruteDamageGroup = "Brute";
    public readonly ProtoId<DamageGroupPrototype> BurnDamageGroup = "Burn";

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();
        _mobstateQuery = GetEntityQuery<MobStateComponent>();
        _flammableQuery = GetEntityQuery<FlammableComponent>();

        SubscribeLocalEvent<FleshmendComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<FleshmendComponent, ComponentRemove>(OnRemoved);
        SubscribeLocalEvent<FleshmendComponent, MobStateChangedEvent>(OnMobStateChange);
        SubscribeLocalEvent<FleshmendComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    private void OnRefresh(EntityUid uid, FleshmendComponent component, RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(component.MovementSpeedDebuff, component.MovementSpeedDebuff);
    }

    private void OnMapInit(Entity<FleshmendComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        if (ent.Comp.Duration.HasValue)
            ent.Comp.MaxDuration = _timing.CurTime + TimeSpan.FromSeconds((double) ent.Comp.Duration);

        Cycle(ent);
    }

    private void OnRemoved(Entity<FleshmendComponent> ent, ref ComponentRemove args)
    {
        if (!_netManager.IsClient)
            RemoveFleshmendEffects(ent);

        if (ent.Comp.AlertId != null)
            _alerts.ClearAlert(ent.Owner, (ProtoId<AlertPrototype>) ent.Comp.AlertId);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<FleshmendComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.MaxDuration < _timing.CurTime && comp.Duration.HasValue)
                RemCompDeferred<FleshmendComponent>(uid);

            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;

            Cycle((uid, comp));
        }
    }

    private void OnMobStateChange(Entity<FleshmendComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            RemComp<FleshmendComponent>(ent);
    }

    private void Cycle(Entity<FleshmendComponent> ent)
    {
        if (_flammableQuery.TryComp(ent, out var flammable) && flammable.OnFire)
        {
            RemoveFleshmendEffects(ent);
            return;
        }

        TryAddFleshmendEffects(ent);
        HealDamage(ent);
    }

    private void HealDamage(Entity<FleshmendComponent> ent)
    {
        var bruteTypes = _proto.Index(BruteDamageGroup);
        var burnTypes = _proto.Index(BurnDamageGroup);

        if (!_damageableQuery.TryComp(ent, out var damage))
            return;

        var bruteDiv = Math.Max(1, bruteTypes.DamageTypes.Count(type =>
            damage.Damage.DamageDict.GetValueOrDefault(type) != FixedPoint2.Zero));

        var burnDiv = Math.Max(1, burnTypes.DamageTypes.Count(type =>
            damage.Damage.DamageDict.GetValueOrDefault(type) != FixedPoint2.Zero));

        var bruteHealAmount = ent.Comp.BruteHeal / bruteDiv;
        var burnHealAmount = ent.Comp.BurnHeal / burnDiv;

        var healSpec = new DamageSpecifier();

        foreach (var brute in bruteTypes.DamageTypes)
            healSpec.DamageDict.Add(brute, bruteHealAmount);

        foreach (var burn in burnTypes.DamageTypes)
            healSpec.DamageDict.Add(burn, burnHealAmount);

        healSpec.DamageDict.Add("Asphyxiation", ent.Comp.AsphyxHeal);

        _dmg.TryChangeDamage(ent.Owner, healSpec, true, false);

        _bloodstream.TryModifyBleedAmount(ent.Owner, ent.Comp.BleedingAdjust);
        _bloodstream.TryModifyBloodLevel(ent.Owner, ent.Comp.BloodLevelAdjust);
    }

    private void TryAddFleshmendEffects(Entity<FleshmendComponent> ent)
    {
        if (ent.Comp.ResPath != ResPath.Empty && ent.Comp.EffectState != null)
        {
            var vfx = EnsureComp<FleshmendEffectComponent>(ent);
            vfx.ResPath = ent.Comp.ResPath;
            vfx.EffectState = ent.Comp.EffectState;
            Dirty(ent, vfx);
        }

        if (ent.Comp.PassiveSound != null && ent.Comp.SoundSource == null)
            DoFleshmendSound(ent);
    }

    private void RemoveFleshmendEffects(Entity<FleshmendComponent> ent)
    {
        RemComp<FleshmendEffectComponent>(ent);
        StopFleshmendSound(ent);
    }

    private void DoFleshmendSound(Entity<FleshmendComponent> ent)
    {
        var audioParams = AudioParams.Default.WithLoop(true).WithVolume(-3f);
        var source = _audio.PlayPredicted(ent.Comp.PassiveSound, ent, null, audioParams);
        ent.Comp.SoundSource = source?.Entity;
    }

    private void StopFleshmendSound(Entity<FleshmendComponent> ent)
    {
        _audio.Stop(ent.Comp.SoundSource);
        ent.Comp.SoundSource = null;
    }
}
