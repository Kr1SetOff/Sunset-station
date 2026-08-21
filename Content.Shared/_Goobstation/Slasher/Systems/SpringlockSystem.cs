using Content.Shared._Goobstation.Slasher.Components;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Inventory;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._Goobstation.Slasher.Systems;

/// <summary>
/// Handles spring-lock clothing that triggers when it or its wearer comes into contact with liquid.
/// </summary>
public sealed class SpringlockSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ReactiveComponent, TouchReactionRelayEvent>(OnReactionEntity);
    }

    private void OnReactionEntity(Entity<ReactiveComponent> ent, ref TouchReactionRelayEvent args)
    {
        if (!HasComp<InventoryComponent>(ent.Owner))
            return;

        var slots = _inventory.GetSlotEnumerator(ent.Owner, SlotFlags.WITHOUT_POCKET);
        while (slots.NextItem(out var item))
        {
            if (!TryComp<SpringlockClothingComponent>(item, out var springlock) || springlock.IsLocked)
                continue;

            springlock.IsLocked = true;
            Dirty(item, springlock);

            _appearance.SetData(item, SpringlockVisuals.Locked, true);
            _audio.PlayPredicted(springlock.LockSound, ent.Owner, ent.Owner);
        }
    }
}
