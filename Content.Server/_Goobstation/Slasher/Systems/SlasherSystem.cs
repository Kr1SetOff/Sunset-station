using Content.Server.Body.Components;
using Content.Shared._Goobstation.Slasher.Components;
using Content.Shared.Body.Components;
using Content.Shared._Starlight.Medical.Body.Part;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.Rejuvenate;
using Content.Shared.Standing;

namespace Content.Server._Goobstation.Slasher.Systems;

/// <summary>
/// Moves the slasher's brain from the head into the chest
/// </summary>
public sealed class SlasherSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherComponent, RejuvenateEvent>(OnRejuvenate);
    }

    private void OnMapInit(Entity<SlasherComponent> ent, ref MapInitEvent args) =>
        MoveBrainToChest(ent);

    private void OnRejuvenate(Entity<SlasherComponent> ent, ref RejuvenateEvent args) =>
        MoveBrainToChest(ent);

    private void MoveBrainToChest(Entity<SlasherComponent> ent)
    {
        if (!TryComp<BodyComponent>(ent, out var bodyComp))
            return;

        var brains = _body.GetBodyOrganEntityComps<BrainComponent>((ent.Owner, bodyComp));
        if (brains.Count == 0)
            return;

        var brain = brains[0];

        EntityUid? chestPart = null;
        // Goob-Station calls this part type "Chest"; in this fork the equivalent enum value is "Torso".
        foreach (var (partId, _) in _body.GetBodyChildrenOfType(ent.Owner, BodyPartType.Torso, bodyComp))
        {
            chestPart = partId;
            break;
        }

        if (chestPart == null)
            return;

        _body.RemoveOrgan(brain.Owner, brain.Comp2);
        _body.TryCreateOrganSlot(chestPart, "brain", out _, null);
        _body.InsertOrgan(chestPart.Value, brain.Owner, "brain");
        _standing.Stand(ent.Owner, force: true);
    }
}
