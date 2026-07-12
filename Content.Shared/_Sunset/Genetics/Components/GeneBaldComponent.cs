// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Humanoid.Markings;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Granted by a genetic mutation: strips the carrier's hair and facial hair while active.
///     <see cref="Content.Server._Sunset.Genetics.GeneBaldSystem"/> removes the markings on activation and
///     restores them again once the gene deactivates.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GeneBaldComponent : Component
{
    /// <summary>Server-side bookkeeping: markings removed on activation, keyed by category, so they can be restored.</summary>
    [ViewVariables]
    public Dictionary<MarkingCategories, List<Marking>> RemovedMarkings = new();
}
