// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Weapons.KnockdownOnHit;

/// <summary>
/// Knocks down whoever this melee weapon hits.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class KnockdownOnHitComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Duration = 4f;

    [DataField, AutoNetworkedField]
    public bool DropItems = true;

    [DataField, AutoNetworkedField]
    public bool Autostand = true;

    /// <summary>
    /// If true, only wide (directional) attacks knock down - this fork has no heavy/windup attack
    /// concept, so wide attacks are the closest equivalent to Goob's "heavy attack".
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool KnockdownOnHeavyAttack;
}
