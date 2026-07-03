// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Granted by a genetic mutation: prevents the carrier from wearing anything in the given inventory
///     slots (e.g. a hulk's huge hands can't fit gloves). Anything already worn there is stripped on
///     activation. <see cref="Content.Shared._Sunset.Genetics.GeneBlockedSlotsSystem"/> enforces it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GeneBlockedSlotsComponent : Component
{
    /// <summary>Inventory slots that cannot be equipped while the gene is active.</summary>
    [DataField, AutoNetworkedField]
    public SlotFlags Slots = SlotFlags.GLOVES;
}
