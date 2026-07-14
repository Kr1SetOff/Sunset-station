using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.TheBoys.Components;

/// <summary>
/// Granted for a limited time by drinking real Compound V while tagged TheBoysKimiko (see
/// CompoundV's reagent effects). TheBoysPowersSystem boosts her punch damage, grants physical
/// damage resistance, and heals her over time for as long as this is present - Kimiko is the
/// strongest of Butcher's team and the only one with a real regenerative healing factor.
/// </summary>
[RegisterComponent]
public sealed partial class TheBoysKimikoPowerComponent : Component
{
    /// <summary>The wearer's DamageModifierSetId from before the power was granted, restored on removal.</summary>
    [DataField]
    public ProtoId<DamageModifierSetPrototype>? PreviousDamageModifierSet;
}
