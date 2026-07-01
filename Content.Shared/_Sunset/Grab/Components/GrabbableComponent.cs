using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Grab.Components;

/// <summary>
/// Placed on an entity while it is being grabbed/pulled by another mob. Tracks grab escalation stage
/// and (at Choke stage) periodic asphyxiation damage.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GrabbableComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityUid? Grabber;

    [DataField, AutoNetworkedField]
    public GrabStage Stage = GrabStage.Passive;

    [DataField, AutoNetworkedField]
    public TimeSpan NextChokeTick = TimeSpan.Zero;

    [DataField]
    public TimeSpan ChokeTickInterval = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Extra delay before the first choke damage tick after just entering the Choke stage
    /// (the grabber needs a moment to get a proper two-handed grip).
    /// </summary>
    [DataField]
    public TimeSpan ChokeStartupDelay = TimeSpan.FromSeconds(1.5);

    [DataField]
    public FixedPoint2 ChokeDamageMin = 2;

    [DataField]
    public FixedPoint2 ChokeDamageMax = 3;

    [DataField]
    public ProtoId<AlertPrototype> AggressiveAlert = "GrabbedAggressive";

    [DataField]
    public ProtoId<AlertPrototype> ChokeAlert = "GrabbedChoke";
}
