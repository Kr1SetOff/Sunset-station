using Content.Shared._Goobstation.SpecialPassives.SuperAdrenaline.Components;
using Content.Shared.Alert;
using Content.Shared.Bed.Sleep;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.SpecialPassives.SuperAdrenaline;

public sealed class SharedSuperAdrenalineSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SleepingSystem _sleep = default!;

    private EntityQuery<StaminaComponent> _staminaQuery;
    private EntityQuery<SleepingComponent> _sleepingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _staminaQuery = GetEntityQuery<StaminaComponent>();
        _sleepingQuery = GetEntityQuery<SleepingComponent>();

        SubscribeLocalEvent<SuperAdrenalineComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SuperAdrenalineComponent, ComponentRemove>(OnRemoved);
        SubscribeLocalEvent<SuperAdrenalineComponent, MobStateChangedEvent>(OnMobStateChange);
    }

    private void OnMapInit(Entity<SuperAdrenalineComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.UpdateTimer = _timing.CurTime + ent.Comp.UpdateDelay;

        if (ent.Comp.Duration.HasValue)
            ent.Comp.MaxDuration = _timing.CurTime + TimeSpan.FromSeconds((double) ent.Comp.Duration);

        Cycle(ent);
    }

    private void OnRemoved(Entity<SuperAdrenalineComponent> ent, ref ComponentRemove args)
    {
        if (ent.Comp.AlertId != null)
            _alerts.ClearAlert(ent.Owner, (ProtoId<AlertPrototype>) ent.Comp.AlertId);
    }

    private void OnMobStateChange(Entity<SuperAdrenalineComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
            RemComp<SuperAdrenalineComponent>(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_timing.IsFirstTimePredicted)
            return;

        var query = EntityQueryEnumerator<SuperAdrenalineComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.MaxDuration < _timing.CurTime && comp.Duration.HasValue)
                RemCompDeferred<SuperAdrenalineComponent>(uid);

            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;

            Cycle((uid, comp));
        }
    }

    private void Cycle(Entity<SuperAdrenalineComponent> ent)
    {
        if (ent.Comp.IgnoreStun)
            RemComp<StunnedComponent>(ent);

        if (ent.Comp.IgnoreKnockdown)
            RemComp<KnockedDownComponent>(ent);

        if (ent.Comp.IgnoreSleep && _sleepingQuery.TryComp(ent, out var sleep))
            _sleep.TryWaking((ent.Owner, sleep), true);

        if (_staminaQuery.TryComp(ent, out var stam))
        {
            stam.StaminaDamage = Math.Clamp(stam.StaminaDamage - ent.Comp.StaminaRegeneration, 0f, stam.CritThreshold);
            Dirty(ent.Owner, stam);
        }

        if (ent.Comp.PassiveDamage != null)
            _damageable.TryChangeDamage(ent.Owner, ent.Comp.PassiveDamage, true, false);
    }
}
