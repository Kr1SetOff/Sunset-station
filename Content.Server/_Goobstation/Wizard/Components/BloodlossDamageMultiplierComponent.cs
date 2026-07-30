// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Server._Goobstation.Wizard.Components;

/// <summary>
/// Applied by the "Scream For Me" spell to multiply bloodloss damage on the target
/// until their bleeding stops. See <see cref="Content.Shared._Goobstation.Bloodstream.GetBloodlossDamageMultiplierEvent"/>
/// for why this is currently inert on this fork.
/// </summary>
[RegisterComponent]
public sealed partial class BloodlossDamageMultiplierComponent : Component
{
    [DataField]
    public float Multiplier = 2f;
}
