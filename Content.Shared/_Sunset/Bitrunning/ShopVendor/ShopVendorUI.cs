// SPDX-License-Identifier: AGPL-3.0-or-later
// 🌇Sunset🌇 - ported from Orion-Station-14 (originally DeltaV), see ShopVendorComponent.cs.

using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.VendingMachines;

[Serializable, NetSerializable]
public sealed class ShopVendorPurchaseMessage(int index) : BoundUserInterfaceMessage
{
    public readonly int Index = index;
}
