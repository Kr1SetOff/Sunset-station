using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Sunset.Flight;

/// <summary>
/// Adds an action that allows the user to become temporarily weightless at the cost of
/// stamina and hand usage. Ported from Goob-Station's Harpy flight mechanic
/// (Content.Shared._EinsteinEngines.Flight.FlightComponent), adapted to this fork's stamina
/// API (no keyed continuous-drain sources here, so SharedFlightSystem drains stamina directly
/// every tick instead of registering a named drain source).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FlightComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? ToggleAction = "ActionToggleFlight";

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    /// <summary>Is the user flying right now?</summary>
    [DataField, AutoNetworkedField]
    public bool On;

    /// <summary>
    /// True if flight itself added PacifiedComponent (i.e. the entity wasn't already Pacified for some
    /// other reason before takeoff) - only then does landing remove it again.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool PacifiedByFlight;

    /// <summary>Stamina drain per second when flying.</summary>
    [DataField, AutoNetworkedField]
    public float StaminaDrainRate = 13.0f;

    /// <summary>By how much do we multiply stamina regen while flying?</summary>
    [DataField, AutoNetworkedField]
    public float StaminaRegenMultiplier = 0.25f;

    /// <summary>How much does this modify the weightless acceleration and speed?</summary>
    [DataField, AutoNetworkedField]
    public float SpeedModifier = 3.0f;

    /// <summary>How much does this modify the weightless friction?</summary>
    [DataField, AutoNetworkedField]
    public float FrictionModifier = 12f;

    /// <summary>How much does this modify the weightless friction when no input is applied?</summary>
    [DataField, AutoNetworkedField]
    public float FrictionNoInputModifier = 40f;

    /// <summary>Sound made periodically while flying.</summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? FlapSound;

    /// <summary>Time between flap sounds, in seconds.</summary>
    [DataField, AutoNetworkedField]
    public float FlapInterval = 1.0f;

    /// <summary>Sound played once on landing (an actual on->off transition, not a failed toggle).</summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier? LandingSound;

    public float TimeUntilFlap;

    /// <summary>Does this flight behavior change collision masks?</summary>
    [DataField, AutoNetworkedField]
    public bool ChangeCollisionMasks = true;

    /// <summary>List of fixtures that had their collision mask changed, so it can be restored.</summary>
    [DataField, AutoNetworkedField]
    public List<(string key, int originalMask)> ChangedFixtures = new();

    /// <summary>Does flight fail (deal damage) when it stops abruptly (e.g. knocked down)?</summary>
    [DataField, AutoNetworkedField]
    public bool CanFail = true;

    /// <summary>Damage applied when flight fails.</summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier FailDamage = new()
    {
        DamageDict = new Dictionary<string, FixedPoint2>
        {
            { "Blunt", 3.5 },
        },
    };
}
