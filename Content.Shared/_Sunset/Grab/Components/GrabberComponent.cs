using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Grab.Components;

/// <summary>
/// Placed on an entity while it is pulling another mob. Tracks grab escalation stage.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrabberComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Grabbing;

    [DataField, AutoNetworkedField]
    public GrabStage Stage = GrabStage.Passive;

    [DataField, AutoNetworkedField]
    public TimeSpan NextEscalation = TimeSpan.Zero;

    [DataField]
    public TimeSpan EscalationCooldown = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Multiplier applied to the vanilla hand-throw speed when throwing a grabbed mob - a mob is much
    /// heavier than a held item, so it shouldn't fly nearly as far/fast.
    /// </summary>
    [DataField]
    public float ThrowSpeedMultiplier = 0.2f;

    /// <summary>
    /// The second virtual item occupying the grabber's other hand while at the Choke stage
    /// (represents choking with both hands). Removed when the grab ends.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid? ChokeVirtualItem;
}
