// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Granted by a genetic mutation: rescales the carrier's sprite while active via the engine
///     <c>ScaleVisuals</c> appearance scale (which bypasses the per-species height clamp).
///     <see cref="Content.Server._Sunset.Genetics.GeneticSizeSystem"/> multiplies the scale on activation and
///     divides it back out on removal. Values above 1 enlarge (gigantism), below 1 shrink (dwarfism).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GeneticSizeComponent : Component
{
    /// <summary>Scale multiplier applied to the carrier's sprite.</summary>
    [DataField, AutoNetworkedField]
    public float Scale = 1.5f;
}
