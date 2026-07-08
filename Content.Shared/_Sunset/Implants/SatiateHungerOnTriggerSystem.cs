using Content.Shared.Nutrition.EntitySystems;
using Content.Shared.Trigger;

namespace Content.Shared._Sunset.Implants;

public sealed class SatiateHungerOnTriggerSystem : XOnTriggerSystem<SatiateHungerOnTriggerComponent>
{
    [Dependency] private readonly HungerSystem _hunger = default!;

    protected override void OnTrigger(Entity<SatiateHungerOnTriggerComponent> ent, EntityUid target, ref TriggerEvent args)
    {
        _hunger.ModifyHunger(target, ent.Comp.HungerAmount);
        args.Handled = true;
    }
}
