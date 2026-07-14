namespace Content.Shared._Sunset.TheBoys.Components;

/// <summary>
/// Granted for a limited time by taking real Compound V while tagged TheBoysHughie (see CompoundV's
/// reagent effects). TheBoysPowersSystem grants a short-range teleport action plus Homelander-grade
/// passive regeneration for as long as this is present - matching the brief super-strength/teleport
/// high Hughie gets on V in the show.
/// </summary>
[RegisterComponent]
public sealed partial class TheBoysHughiePowerComponent : Component
{
    /// <summary>The granted blink action entity, tracked here so it can be removed again on shutdown
    /// regardless of whether it was ever activated.</summary>
    [ViewVariables]
    public EntityUid? GrantedAction;
}
