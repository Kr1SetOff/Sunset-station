using Content.Shared._Sunset.HotlineMiami;
using Robust.Shared.Containers;

namespace Content.Server._Sunset.HotlineMiami;

public sealed class TakeOneItemStorageSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TakeOneItemStorageComponent, EntRemovedFromContainerMessage>(OnRemoved);
    }

    private void OnRemoved(Entity<TakeOneItemStorageComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (ent.Comp.Triggered)
            return;

        if (args.Container.ID != ent.Comp.ContainerId)
            return;

        ent.Comp.Triggered = true;

        var remaining = new List<EntityUid>(args.Container.ContainedEntities);
        foreach (var contained in remaining)
        {
            QueueDel(contained);
        }
    }
}
