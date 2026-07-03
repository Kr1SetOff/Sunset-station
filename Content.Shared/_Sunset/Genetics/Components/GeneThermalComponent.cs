// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Granted by a genetic mutation: protects the carrier from heat and cold damage by widening their
///     temperature damage thresholds while active. <see cref="Content.Server._Sunset.Genetics.GeneThermalSystem"/>
///     stores the original thresholds on activation and restores them on removal.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GeneThermalComponent : Component
{
    /// <summary>
    ///     When true, only cold is ignored (the heat threshold is left untouched). Used by the cryogenesis
    ///     gene, which protects against cold and pressure but not heat.
    /// </summary>
    [DataField]
    public bool ColdOnly;

    [DataField]
    public float OldHeatThreshold;

    [DataField]
    public float OldColdThreshold;

    [DataField]
    public bool Stored;
}
