// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Sunset.Arena.Components;

/// <summary>
/// Marker placed on entities (via the ArenaSpawner prototype) that denote where arena fighters spawn.
/// Used only when a hand-made Resources/Maps/arena.yml is loaded instead of the generated arena.
/// </summary>
[RegisterComponent]
public sealed partial class ArenaSpawnComponent : Component
{
}
