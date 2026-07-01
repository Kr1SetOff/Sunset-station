using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Kung Fu Dragon's passive: standing still for a moment builds up power, granting a temporary damage
/// buff on your next attacks.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class DragonPowerComponent : Component
{
    [ViewVariables]
    public TimeSpan LastMoveTime;

    [DataField]
    public float MinVelocitySquared = 0.25f;

    [DataField]
    public TimeSpan PauseDuration = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan BuffLength = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public TimeSpan PowerBuffUntil = TimeSpan.Zero;

    [DataField]
    public float DamageMultiplier = 1.25f;
}
