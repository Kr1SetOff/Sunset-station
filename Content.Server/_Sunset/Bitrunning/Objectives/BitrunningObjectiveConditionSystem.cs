using Content.Shared._Sunset.Bitrunning;
using Content.Shared._Sunset.Bitrunning.Components;
using Content.Shared.Objectives.Components;
using Robust.Server.GameObjects;

namespace Content.Server._Sunset.Bitrunning.Objectives;

/// <summary>
/// Sets the title/description/progress of a Bitrunning domain-run objective from the linked
/// QuantumServerComponent, so a domain's goal ("collect 3 caches", "eliminate 8 threats", ...)
/// shows up as a real, checkable objective on the avatar's character sheet - see
/// QuantumServerSystem for where this gets attached to the avatar's mind on connect.
/// </summary>
public sealed class BitrunningObjectiveConditionSystem : EntitySystem
{
    [Dependency] private readonly MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BitrunningObjectiveConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
        SubscribeLocalEvent<BitrunningObjectiveConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
    }

    private void OnAfterAssign(Entity<BitrunningObjectiveConditionComponent> ent, ref ObjectiveAfterAssignEvent args)
    {
        if (ent.Comp.Server is not { } server || !TryComp<QuantumServerComponent>(server, out var quantum))
            return;

        var title = GetObjectiveInstructions(quantum);
        _metaData.SetEntityName(ent.Owner, title, args.Meta);
        _metaData.SetEntityDescription(ent.Owner, title, args.Meta);
    }

    private void OnGetProgress(Entity<BitrunningObjectiveConditionComponent> ent, ref ObjectiveGetProgressEvent args)
    {
        if (ent.Comp.Server is not { } server || !TryComp<QuantumServerComponent>(server, out var quantum))
        {
            args.Progress = 0f;
            return;
        }

        if (quantum.ObjectiveCompleted)
        {
            args.Progress = 1f;
            return;
        }

        args.Progress = quantum.ObjectiveGoal > 0
            ? Math.Clamp(quantum.ObjectivePoints / (float) quantum.ObjectiveGoal, 0f, 1f)
            : 0f;
    }

    /// <summary>Same switch as QuantumServerSystem.GetObjectiveInstructions - kept in sync manually
    /// since that one's private and this system needs the exact same wording for the objective text.</summary>
    private string GetObjectiveInstructions(QuantumServerComponent server)
    {
        var target = server.ObjectiveGoal.ToString();
        return server.ObjectiveType switch
        {
            BitrunningObjectiveType.None => Loc.GetString("bitrunning-training-instructions-none"),
            BitrunningObjectiveType.CollectEncryptedCaches => Loc.GetString("bitrunning-training-instructions-collect", ("target", target)),
            BitrunningObjectiveType.DeliveryCacheCrate => Loc.GetString("bitrunning-training-instructions-delivery", ("target", target)),
            BitrunningObjectiveType.EliminateEnemies => Loc.GetString("bitrunning-training-instructions-eliminate", ("target", target)),
            BitrunningObjectiveType.CatchFish => Loc.GetString("bitrunning-training-instructions-catch-fish", ("target", target)),
            BitrunningObjectiveType.FillStomach => Loc.GetString("bitrunning-training-instructions-fill-stomach", ("target", target)),
            BitrunningObjectiveType.OverhydrateStomach => Loc.GetString("bitrunning-training-instructions-overhydrate-stomach", ("target", target)),
            _ => Loc.GetString("bitrunning-training-instructions-none"),
        };
    }
}
