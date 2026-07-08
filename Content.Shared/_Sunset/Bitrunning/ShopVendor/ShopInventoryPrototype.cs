// SPDX-License-Identifier: AGPL-3.0-or-later
// 🌇Sunset🌇 - ported from Orion-Station-14 (originally DeltaV), see ShopVendorComponent.cs.

using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.VendingMachines;

/// <summary>
/// Similar to <c>VendingMachineInventoryPrototype</c> but for <see cref="ShopVendorComponent"/>.
/// </summary>
[Prototype("shopInventory")]
public sealed partial class ShopInventoryPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>
    /// The item listings for sale.
    /// </summary>
    [DataField(required: true)]
    public List<ShopListing> Listings = new();
}

[DataRecord, Serializable]
public partial record struct ShopListing(EntProtoId Id, uint Cost, LocId? OverrideName = null);
