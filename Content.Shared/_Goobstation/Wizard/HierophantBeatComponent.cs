// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Wizard;

/// <summary>
///     Ported from Goob-Station's Lavaland content for the "Toggle Tile Movement" wizard spell.
///     Simplified: this fork has no TileMovementComponent (a "_vg"-sourced grid-snap movement mode),
///     so this just applies the movement speed buff/alert without the tile-snapping behavior.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HierophantBeatComponent : Component
{
    [DataField]
    public float MovementSpeedBuff = 1.25f;

    [DataField]
    public ProtoId<AlertPrototype> HierophantBeatAlertId = "HierophantBeat";
}
