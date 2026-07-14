using Content.Server.Objectives.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// 🌇Sunset🌇 - handles progress for Homelander's "seize control of the station" objective: he must
/// survive to the end of the shift, and nobody else may escape on the emergency shuttle.
/// </summary>
public sealed partial class HomelanderControlStationConditionSystem : EntitySystem
{
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly EmergencyShuttleSystem _emergencyShuttle = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HomelanderControlStationConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnGetProgress(EntityUid uid, HomelanderControlStationConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        if (_mind.IsCharacterDeadIc(args.Mind))
        {
            args.Progress = 0f;
            return;
        }

        // Nobody - crew or Homelander himself - may have made it onto the emergency shuttle once it
        // arrives. Sitting in the station's cryo/escape pods doesn't count (IsTargetEscaping only
        // returns true for the actual emergency shuttle grid, and only once evac has arrived).
        var query = EntityQueryEnumerator<MindContainerComponent>();
        while (query.MoveNext(out var body, out var container))
        {
            if (!container.HasMind)
                continue;

            if (_emergencyShuttle.IsTargetEscaping(body))
            {
                args.Progress = 0f;
                return;
            }
        }

        args.Progress = 1f;
    }
}
