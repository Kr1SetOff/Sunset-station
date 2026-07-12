// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Granted by a genetic mutation: blurs the carrier's vision without fully blinding them.
///     Kept as its own component (rather than reusing <c>PermanentBlindness</c>) so this gene can be active
///     alongside <c>GeneBlindness</c> without the two stomping on each other's activation/deactivation.
///     <see cref="Content.Shared._Sunset.Genetics.GeneNearsightedSystem"/> drives the eye-damage floor.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GeneNearsightedComponent : Component
{
    /// <summary>Eye damage floor applied while active, kept under BlindableComponent.MaxDamage so it blurs rather than blinds.</summary>
    [DataField, AutoNetworkedField]
    public int Blindness = 4;
}
