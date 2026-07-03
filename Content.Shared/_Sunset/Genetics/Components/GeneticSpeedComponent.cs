// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Granted by a genetic mutation: multiplies the carrier's walk and sprint speed while active.
///     Added/removed by the mutation's component registry; the speed change is applied through the
///     standard <c>RefreshMovementSpeedModifiersEvent</c> relay.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GeneticSpeedComponent : Component
{
    /// <summary>Walk speed multiplier. Values above 1 speed the carrier up, below 1 slow them.</summary>
    [DataField, AutoNetworkedField]
    public float WalkModifier = 1.25f;

    /// <summary>Sprint speed multiplier.</summary>
    [DataField, AutoNetworkedField]
    public float SprintModifier = 1.25f;
}
