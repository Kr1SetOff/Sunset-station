// SPDX-License-Identifier: AGPL-3.0-or-later
// 🌇Sunset🌇 - ported from Orion-Station-14.

using Content.Shared._Sunset.VendingMachines;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Bitrunning.Components;

/// <summary>
/// Makes a <see cref="ShopVendorComponent"/> use bitrunning points to buy items.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BitrunningPointsVendorComponent : Component;
