using Content.Server.Objectives.Components;
using Content.Shared._Sunset.BloodWorm;
using Content.Shared.Objectives.Components;
using Robust.Shared.GameObjects;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// 🌇Sunset🌇 - handles progress for the blood worm's Consume objective.
/// </summary>
public sealed partial class BloodWormConsumeConditionSystem : EntitySystem
{
    [Dependency] private MetaDataSystem _metaData = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodWormConsumeConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
        SubscribeLocalEvent<BloodWormConsumeConditionComponent, ObjectiveAfterAssignEvent>(OnAfterAssign);
    }

    private void OnGetProgress(EntityUid uid, BloodWormConsumeConditionComponent comp, ref ObjectiveGetProgressEvent args)
    {
        if (args.Mind.OwnedEntity is not { } owned || !TryComp<BloodWormComponent>(owned, out var worm))
        {
            args.Progress = 0f;
            return;
        }

        args.Progress = comp.Amount <= 0f ? 1f : Math.Clamp(worm.ConsumedBlood / comp.Amount, 0f, 1f);
    }

    // 🌇Sunset🌇 - the title/description reference {$amount}, which Robust only fills in if
    // something explicitly calls Loc.GetString with it; the plain name/description fields in
    // objectives.yml are just unresolved fallback text otherwise (shown to players as raw Loc IDs).
    private void OnAfterAssign(EntityUid uid, BloodWormConsumeConditionComponent comp, ref ObjectiveAfterAssignEvent args)
    {
        var amount = (int) comp.Amount;
        _metaData.SetEntityName(uid, Loc.GetString("objective-condition-blood-worm-consume-title", ("amount", amount)), args.Meta);
        _metaData.SetEntityDescription(uid, Loc.GetString("objective-condition-blood-worm-consume-description", ("amount", amount)), args.Meta);
    }
}
