// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Changeling.Components;

/// <summary>
///     Component that indicates that a person can be absorbed by a changeling, but will not give any objective progress or evolution points.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PartialAbsorbableComponent : Component
{

}
