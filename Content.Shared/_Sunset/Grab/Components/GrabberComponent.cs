using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

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
    public TimeSpan EscalationCooldown = TimeSpan.FromSeconds(1.5);

    [DataField, AutoNetworkedField]
    public EntityUid? ThrowActionEntity;

    [DataField]
    public EntProtoId ThrowActionId = "ActionGrabThrow";

    [DataField]
    public float ThrowSpeed = 6.5f;
}
