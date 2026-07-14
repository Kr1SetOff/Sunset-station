using Content.Server.Objectives.Systems;

namespace Content.Server.Objectives.Components;

/// <summary>
/// 🌇Sunset🌇 - Homelander's "seize control of the station" objective. Completed only if he's alive
/// AND nobody else has escaped on the emergency shuttle by the time the round ends - checked
/// together in one condition rather than as two separate stacked conditions, since two components on
/// the same objective entity both handling ObjectiveGetProgressEvent would just overwrite each
/// other's Progress instead of combining (see HomelanderControlStationConditionSystem).
/// </summary>
[RegisterComponent, Access(typeof(HomelanderControlStationConditionSystem))]
public sealed partial class HomelanderControlStationConditionComponent : Component
{
}
