using Content.Shared._Sunset.Chemistry.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.Chemistry;

/// <summary>
/// 🌇Sunset🌇 - runtime half of the BluespaceDistorter reagent: while BluespacePhaseComponent is
/// present, every fixture on the entity is made non-hard (collisions stop blocking), so it walks
/// straight through walls; everything is restored when the effect expires or the component is
/// removed for any reason.
/// </summary>
public sealed class BluespacePhaseSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BluespacePhaseComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<BluespacePhaseComponent, ComponentShutdown>(OnShutdown);
        // 🌇Sunset🌇 - phasing is for slipping through walls, not for attacking from inside them: any
        // attack attempt drops the phase immediately (also used by the Nar'Sie Wraith construct's ability).
        SubscribeLocalEvent<BluespacePhaseComponent, AttackAttemptEvent>(OnAttackAttempt);
    }

    private void OnAttackAttempt(Entity<BluespacePhaseComponent> ent, ref AttackAttemptEvent args)
    {
        RemCompDeferred<BluespacePhaseComponent>(ent.Owner);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<BluespacePhaseComponent>();
        while (query.MoveNext(out var uid, out var phase))
        {
            if (now >= phase.EndTime)
                RemComp<BluespacePhaseComponent>(uid);
        }
    }

    private void OnStartup(Entity<BluespacePhaseComponent> ent, ref ComponentStartup args)
    {
        SetHardAll(ent, false);
        _popup.PopupEntity(Loc.GetString("sunset-bluespace-phase-start"), ent, ent, PopupType.Medium);
    }

    private void OnShutdown(Entity<BluespacePhaseComponent> ent, ref ComponentShutdown args)
    {
        SetHardAll(ent, true);
        _popup.PopupEntity(Loc.GetString("sunset-bluespace-phase-end"), ent, ent, PopupType.Medium);
    }

    private void SetHardAll(EntityUid uid, bool hard)
    {
        if (!TryComp<FixturesComponent>(uid, out var fixtures))
            return;

        foreach (var fixture in fixtures.Fixtures.Values)
        {
            _physics.SetHard(uid, fixture, hard, fixtures);
        }
    }
}
