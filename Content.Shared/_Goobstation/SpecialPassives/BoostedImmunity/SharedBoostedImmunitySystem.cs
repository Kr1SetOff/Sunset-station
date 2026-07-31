using System.Linq;
using Content.Shared._Goobstation.SpecialPassives.BoostedImmunity.Components;
using Content.Shared._Starlight.Medical.Body.Systems;
using Content.Shared.Alert;
using Content.Shared.CombatMode.Pacification;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Damage.Systems;
using Content.Shared.Drunk;
using Content.Shared.Eye.Blinding.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.SpecialPassives.BoostedImmunity;

public sealed class SharedBoostedImmunitySystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly BlindableSystem _blindSys = default!;
    [Dependency] private readonly DamageableSystem _dmg = default!;
    [Dependency] private readonly SharedDrunkSystem _drunkSys = default!;
    [Dependency] private readonly StatusEffectsSystem _status = default!;
    [Dependency] private readonly SharedBloodstreamSystem _bloodSys = default!;

    private EntityQuery<DamageableComponent> _damageableQuery;
    private EntityQuery<MobStateComponent> _mobStateQuery;
    private EntityQuery<StatusEffectsComponent> _statusQuery;

    public readonly ProtoId<DamageGroupPrototype> ToxinDamageGroup = "Toxin";

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();
        _statusQuery = GetEntityQuery<StatusEffectsComponent>();

        SubscribeLocalEvent<BoostedImmunityComponent, ComponentStartup>(OnMapInit);
        SubscribeLocalEvent<BoostedImmunityComponent, ComponentRemove>(OnRemoved);
        SubscribeLocalEvent<BoostedImmunityComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    private void OnMapInit(Entity<BoostedImmunityComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        if (ent.Comp.Duration.HasValue)
            ent.Comp.MaxDuration = _timing.CurTime + TimeSpan.FromSeconds((double) ent.Comp.Duration);

        if (_mobStateQuery.TryComp(ent, out var state))
            ent.Comp.Mobstate = state.CurrentState;

        Cycle(ent);
    }

    private void OnRemoved(Entity<BoostedImmunityComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.AlertId != null)
            _alerts.ClearAlert(ent.Owner, (ProtoId<AlertPrototype>) ent.Comp.AlertId);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<BoostedImmunityComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.MaxDuration < _timing.CurTime && comp.Duration.HasValue)
                RemCompDeferred<BoostedImmunityComponent>(uid);

            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;

            Cycle((uid, comp));
        }
    }

    private void OnMobStateChange(Entity<BoostedImmunityComponent> ent, ref MobStateChangedEvent args)
    {
        ent.Comp.Mobstate = args.NewMobState;
    }

    private void Cycle(Entity<BoostedImmunityComponent> ent)
    {
        if (ent.Comp.Mobstate == MobState.Dead && !ent.Comp.WorkWhileDead)
        {
            RemComp<BoostedImmunityComponent>(ent);
            return;
        }

        if (_statusQuery.TryComp(ent, out var status))
        {
            if (ent.Comp.ApplySober)
                _drunkSys.TryRemoveDrunkenness(ent);

            if (ent.Comp.RemovePacifism)
            {
                _status.TryRemoveStatusEffect(ent, "Pacified", status);
                RemComp<PacifiedComponent>(ent);
            }
        }

        if (ent.Comp.CleanseChemicals)
            _bloodSys.FlushChemicals(ent.Owner, ent.Comp.CleanseChemicalsAmount, null);

        HealDamage(ent);
        _blindSys.AdjustEyeDamage(ent.Owner, -ent.Comp.EyeDamageHeal);
    }

    private void HealDamage(Entity<BoostedImmunityComponent> ent)
    {
        var toxinTypes = _proto.Index(ToxinDamageGroup);

        if (!_damageableQuery.TryComp(ent, out var damage))
            return;

        var toxinDiv = Math.Max(1, toxinTypes.DamageTypes.Count(type =>
            damage.Damage.DamageDict.GetValueOrDefault(type) != FixedPoint2.Zero));

        var toxinHealAmount = ent.Comp.ToxinHeal / toxinDiv;

        var healSpec = new DamageSpecifier();
        foreach (var tox in toxinTypes.DamageTypes)
            healSpec.DamageDict.Add(tox, toxinHealAmount);

        _dmg.TryChangeDamage(ent.Owner, healSpec, true, false);
    }
}
