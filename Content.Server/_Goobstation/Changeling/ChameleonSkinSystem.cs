using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Changeling.Systems;
using Content.Server.Polymorph.Systems;
using Content.Shared.Polymorph;

namespace Content.Server._Goobstation.Changeling;

public sealed partial class ChameleonSkinSystem : SharedChameleonSkinSystem
{
    [Dependency] private readonly PolymorphSystem _polymorph = default!;

    private EntityQuery<ChangelingIdentityComponent> _lingQuery;

    public override void Initialize()
    {
        base.Initialize();

        _lingQuery = GetEntityQuery<ChangelingIdentityComponent>();

        SubscribeLocalEvent<ChameleonSkinComponent, PolymorphedEvent>(OnPolymorphed);
    }

    private void OnPolymorphed(Entity<ChameleonSkinComponent> ent, ref PolymorphedEvent args)
    {
        if (_lingQuery.TryComp(ent, out var ling)
            && ling.IsInLastResort)
            return;

        _polymorph.CopyPolymorphComponent<ChameleonSkinComponent>(ent, args.NewEntity);
    }
}
