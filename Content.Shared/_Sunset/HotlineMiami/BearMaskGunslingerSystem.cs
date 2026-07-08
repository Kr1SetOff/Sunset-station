using Content.Shared._Sunset.HoloCigar;
using Content.Shared.Inventory.Events;

namespace Content.Shared._Sunset.HotlineMiami;

/// <summary>
/// Grants/revokes <see cref="HoloCigarWearerComponent"/> while the bear mask is worn - reuses
/// HoloCigarSystem's existing "fire one dual-wielded gun, the other fires too" logic verbatim, no
/// changes needed there since it already keys off the marker component generically. Since both the
/// bear mask and a lit HoloCigar occupy the "mask" slot, they can never be worn at once, so there's
/// no risk of one clobbering the other's grant.
/// </summary>
public sealed class BearMaskGunslingerSystem : EntitySystem
{
    private const string MaskSlot = "mask";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BearMaskGunslingerComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<BearMaskGunslingerComponent, GotUnequippedEvent>(OnUnequipped);
    }

    private void OnEquipped(Entity<BearMaskGunslingerComponent> ent, ref GotEquippedEvent args)
    {
        if (args.Slot != MaskSlot)
            return;

        EnsureComp<HoloCigarWearerComponent>(args.Equipee);
    }

    private void OnUnequipped(Entity<BearMaskGunslingerComponent> ent, ref GotUnequippedEvent args)
    {
        if (args.Slot != MaskSlot)
            return;

        RemComp<HoloCigarWearerComponent>(args.Equipee);
    }
}
