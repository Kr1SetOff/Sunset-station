using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Changeling.Systems;
using Content.Shared._Goobstation.LightDetection.Components;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Changeling;

public sealed partial class DarknessAdaptionSystem : SharedDarknessAdaptionSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();

        SubscribeLocalEvent<DarknessAdaptionComponent, PolymorphedEvent>(OnPolymorphed);
    }

    // unfortunately this can't be moved to shared and predicted without causing issues. so joever.
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<DarknessAdaptionComponent, LightDetectionComponent>();
        while (query.MoveNext(out var uid, out var comp, out var lightComp))
        {
            if (_timing.CurTime < comp.UpdateTimer)
                continue;

            comp.UpdateTimer = _timing.CurTime + comp.UpdateDelay;
            Dirty(uid, comp);

            if (!comp.Active)
                continue;

            DoAbility((uid, comp), !lightComp.OnLight);
        }
    }

    private void OnPolymorphed(Entity<DarknessAdaptionComponent> ent, ref PolymorphedEvent args)
    {
        if (_lingQuery.TryComp(ent, out var ling)
            && ling.IsInLastResort)
            return;

        _polymorph.CopyPolymorphComponent<DarknessAdaptionComponent>(ent, args.NewEntity);
    }
}
