using Content.Shared.Damage;

namespace Content.Shared._Sunset.TheBoys.Components;

/// <summary>
/// Granted for a limited time by taking real Compound V while tagged TheBoysButcher (see CompoundV's
/// reagent effects). TheBoysPowersSystem grants a twin-laser "heat vision" channel (see
/// TheBoysButcherLaserEyesEvent) plus Homelander-grade passive regeneration for as long as this is
/// present - Butcher doesn't take V in the show, but the mode gives every powered protagonist a
/// temporary boost from it rather than only some of the team.
///
/// The laser-channel fields below mirror HomelanderComponent's own (see its doc comments for why
/// each exists) - duplicated rather than shared because granting Butcher an actual HomelanderComponent
/// would also make him emit Homelander's fear aura and get caught by anything that treats
/// HomelanderComponent as "this is really Homelander" (e.g. the Butcher's-crowbar bonus-damage check).
/// </summary>
[RegisterComponent]
public sealed partial class TheBoysButcherPowerComponent : Component
{
    [ViewVariables]
    public bool LaserActive;

    [ViewVariables]
    public TimeSpan LaserEndTime;

    [ViewVariables]
    public float LaserTickAccumulator;

    [ViewVariables]
    public float LaserVisualAccumulator;

    [ViewVariables]
    public DamageSpecifier LaserDamagePerSecond = new();

    [ViewVariables]
    public float LaserRange;

    [ViewVariables]
    public float LaserEyeOffset;

    [ViewVariables]
    public TimeSpan LaserLockout;

    [ViewVariables]
    public EntityUid? LaserActionEntity;

    [ViewVariables]
    public EntityUid? LaserSoundEntity;

    /// <summary>The granted laser-eyes action entity, tracked here so it can be removed again on
    /// shutdown regardless of whether it was ever activated.</summary>
    [ViewVariables]
    public EntityUid? GrantedAction;
}
