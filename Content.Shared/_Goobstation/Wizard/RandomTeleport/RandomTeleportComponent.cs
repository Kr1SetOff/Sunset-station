// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Destructible.Thresholds;
using Robust.Shared.Audio;

namespace Content.Shared._Goobstation.Wizard.RandomTeleport;

/// <summary>
///     Ported from Goob-Station's general-purpose SharedRandomTeleportSystem, simplified: this fork
///     doesn't have Goob's BlockTeleport/GrabIntent/Sparks frameworks, so the pulled-entity-follows-you
///     and spark-VFX behavior were dropped, keeping just the "teleport to a random valid nearby tile" core.
/// </summary>
[RegisterComponent]
public sealed partial class RandomTeleportComponent : Component
{
    [DataField]
    public MinMax Radius = new(0, 6);

    [DataField]
    public int TeleportAttempts = 10;

    [DataField]
    public bool ForceSafeTeleport = true;

    [DataField]
    public SoundSpecifier? DepartureSound;

    [DataField]
    public SoundSpecifier? ArrivalSound;
}
