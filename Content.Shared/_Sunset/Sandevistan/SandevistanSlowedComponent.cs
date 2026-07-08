using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Sandevistan;

/// <summary>
/// Applied to mobs caught in a sandevistan slowfield.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SandevistanSlowedComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid Source;

    [DataField, AutoNetworkedField]
    public float SpeedMultiplier = 1f;

    /// <summary>
    /// Whether this entity is currently actively slowed. False means the slowdown was removed but
    /// the component is pending cleanup.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IsSlowed = true;
}
