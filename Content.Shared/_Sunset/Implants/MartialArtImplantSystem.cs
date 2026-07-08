using Content.Shared._Sunset.MartialArts.Systems;
using Content.Shared.Implants;

namespace Content.Shared._Sunset.Implants;

public sealed class MartialArtImplantSystem : EntitySystem
{
    [Dependency] private readonly SharedMartialArtsSystem _martialArts = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MartialArtImplantComponent, ImplantImplantedEvent>(OnImplanted);
    }

    private void OnImplanted(Entity<MartialArtImplantComponent> ent, ref ImplantImplantedEvent args)
    {
        _martialArts.TryGrantMartialArt(args.Implanted, ent.Comp.Style);
    }
}
