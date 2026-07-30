using Content.Shared._Starlight.Overlay.Components;
using Content.Shared.Actions;
using Content.Shared.Eye.Blinding.Components;

namespace Content.Shared._Sunset.Spy;

/// <summary>
/// SharedThermalVisionSystem only grants ActionToggleThermal from MapInitEvent, which fires once at
/// an entity's own spawn - fine for Homelander/Changeling (granted via AntagSelectionDefinition at
/// IntraPlayerSpawn, before their body's own MapInitEvent has fired), but never fires again for a
/// body that's been up and running for a while, like a crew member putting on the Spy's thermal
/// goggles mid-round (ClothesThermalVisionSystem). MapInitEvent and ComponentShutdown are already claimed by
/// SharedThermalVisionSystem, and ComponentStartup is already claimed by FlashImmunitySystem (see its
/// own "something else is already using it" comment) - every lifecycle event slot on this component
/// is spoken for, so this polls once a tick instead of trying to grab one, same workaround used
/// elsewhere in this fork for the same one-subscriber-per-(component,event) restriction. AddAction
/// itself is idempotent on an already-populated ActionEntity (EnsureAction), so this is a no-op for
/// every entity that already got its action through the normal MapInitEvent path.
/// </summary>
public sealed class ThermalVisionLateGrantSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;

    private const string ToggleAction = "ActionToggleThermal";

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<ThermalVisionComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.ActionEntity != null)
                continue;

            _actions.AddAction(uid, ref comp.ActionEntity, ToggleAction);
        }
    }
}
