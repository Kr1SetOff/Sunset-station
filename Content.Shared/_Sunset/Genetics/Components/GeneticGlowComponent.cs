// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Granted by a genetic mutation: makes the carrier emit light. Added/removed by the mutation's
///     component registry. <see cref="Content.Shared._Sunset.Genetics.GeneticGlowSystem"/> drives the point light.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GeneticGlowComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Radius = 3f;

    [DataField, AutoNetworkedField]
    public float Energy = 2.5f;

    [DataField, AutoNetworkedField]
    public Color Color = Color.FromHex("#88ff88");

    /// <summary>True once this gene created/took over the carrier's point light, so we know to clean it up.</summary>
    [DataField, AutoNetworkedField]
    public bool AddedLight;
}
