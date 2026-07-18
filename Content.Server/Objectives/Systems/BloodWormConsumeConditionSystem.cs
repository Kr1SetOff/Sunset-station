using Content.Server.Objectives.Components;
using Content.Shared._Sunset.BloodWorm;
using Content.Shared.Objectives.Components;

namespace Content.Server.Objectives.Systems;

/// <summary>
/// 🌇Sunset🌇 - handles progress for the blood worm's Consume objective.
/// </summary>
public sealed partial class BloodWormConsumeConditionSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BloodWormConsumeConditionComponent, ObjectiveGetProgressEvent>(OnGetProgress);
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
}
