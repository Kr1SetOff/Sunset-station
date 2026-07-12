// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Granted by a genetic mutation: swells the carrier's fingers until they can no longer manage fine
///     manipulation. <see cref="Content.Shared._Sunset.Genetics.GeneChunkyFingersSystem"/> strips their
///     <see cref="Content.Shared.Interaction.Components.ComplexInteractionComponent"/> while active.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GeneChunkyFingersComponent : Component
{
    /// <summary>True if this gene removed ComplexInteraction itself, so it knows to restore it on removal.</summary>
    [ViewVariables]
    public bool RemovedComplexInteraction;
}
