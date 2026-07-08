using Content.Shared.Clothing.Components;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.Inventory.Events;

namespace Content.Shared._Sunset.Spy;

/// <summary>
/// Grants/removes ThermalVisionComponent when a ClothesThermalVisionComponent item is worn in its
/// clothing slot - mirror of the night-vision-only ClothesVisionSystem. The toggle action for the
/// freshly-added component comes from <see cref="ThermalVisionLateGrantSystem"/> (the normal grant
/// path only fires at the wearer's own MapInit), and removal cleanup (action + overlay off) is
/// SharedThermalVisionSystem's own ComponentShutdown handler.
/// </summary>
public sealed class ClothesThermalVisionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ClothesThermalVisionComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<ClothesThermalVisionComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(Entity<ClothesThermalVisionComponent> ent, ref GotEquippedEvent args)
    {
        if (!TryComp<ClothingComponent>(ent.Owner, out var clothing)
            || !clothing.Slots.HasFlag(args.SlotFlags))
            return;

        if (HasComp<ThermalVisionComponent>(args.Equipee))
        {
            ent.Comp.Granted = false;
            return;
        }

        EnsureComp<ThermalVisionComponent>(args.Equipee);
        ent.Comp.Granted = true;
    }

    private void OnUnequipped(Entity<ClothesThermalVisionComponent> ent, ref GotUnequippedEvent args)
    {
        if (!ent.Comp.Granted)
            return;

        ent.Comp.Granted = false;
        RemComp<ThermalVisionComponent>(args.Equipee);
    }
}
