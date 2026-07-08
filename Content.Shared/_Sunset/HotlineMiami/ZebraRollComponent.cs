using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.HotlineMiami;

/// <summary>
/// Granted to the wearer while all three zebra costume pieces (mask, jacket, tank top) are equipped
/// at once. Tracks the granted roll action and the roll's active state - see ZebraRollSystem.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ZebraRollComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? RollAction;

    /// <summary>Whether a roll is currently active - grants bullet immunity and a speed boost while true.</summary>
    [DataField, AutoNetworkedField]
    public bool Rolling;

    [DataField, AutoNetworkedField]
    public TimeSpan RollEndTime;

    [DataField]
    public TimeSpan NextAfterimageTime;

    [DataField]
    public float AfterimageInterval = 0.1f;

    [DataField]
    public float ColorAccumulator;

    /// <summary>Walk/sprint speed multiplier while rolling.</summary>
    [DataField]
    public float RollSpeedMultiplier = 1.6f;

    [DataField]
    public TimeSpan RollDuration = TimeSpan.FromSeconds(5);
}
