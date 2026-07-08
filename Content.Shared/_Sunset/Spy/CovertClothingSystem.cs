using Content.Shared.Strip.Components;

namespace Content.Shared._Sunset.Spy;

/// <summary>
/// Cancels any strip attempt targeting an item marked with CovertClothingComponent - used for the
/// Spy's covert armor, which should be visible on the wearer but unstrippable/unidentifiable by
/// anyone else going through their inventory. StripAttemptEvent is raised on the person being
/// stripped (not the item), so this subscribes unfiltered and checks args.Item itself rather than
/// requiring CovertClothingComponent on the target.
/// </summary>
public sealed class CovertClothingSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<StripAttemptEvent>(OnStripAttempt);
    }

    private void OnStripAttempt(ref StripAttemptEvent args)
    {
        if (HasComp<CovertClothingComponent>(args.Item))
            args.Cancel();
    }
}
