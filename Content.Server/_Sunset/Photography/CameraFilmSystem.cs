using Content.Shared._Sunset.Photography;
using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Robust.Shared.Containers;

namespace Content.Server._Sunset.Photography;

/// <summary>
/// Handles reloading a camera's <see cref="LimitedChargesComponent"/> by inserting a film cartridge
/// into its film slot. The cartridge is consumed on insert.
/// </summary>
public sealed partial class CameraFilmSystem : EntitySystem
{
    [Dependency] private SharedChargesSystem _charges = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CameraFilmSlotComponent, EntInsertedIntoContainerMessage>(OnFilmInserted);
    }

    private void OnFilmInserted(Entity<CameraFilmSlotComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != ent.Comp.SlotId)
            return;

        if (!TryComp<CameraFilmCartridgeComponent>(args.Entity, out var cartridge))
            return;

        if (TryComp<LimitedChargesComponent>(ent.Owner, out var charges))
            _charges.AddCharges((ent.Owner, charges, null), cartridge.Charges);

        QueueDel(args.Entity);
    }
}
