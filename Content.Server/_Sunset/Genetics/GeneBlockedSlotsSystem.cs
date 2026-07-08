// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     Enforces <see cref="GeneBlockedSlotsComponent"/>: strips items from the blocked slots when the gene
///     activates and rejects any attempt to equip into them while it is active.
/// </summary>
public sealed class GeneBlockedSlotsSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneBlockedSlotsComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GeneBlockedSlotsComponent, IsEquippingTargetAttemptEvent>(OnEquipAttempt);
    }

    private void OnStartup(Entity<GeneBlockedSlotsComponent> ent, ref ComponentStartup args)
    {
        if (!_inventory.TryGetSlots(ent, out var slots))
            return;

        // Drop anything already worn in a now-blocked slot.
        foreach (var slot in slots)
        {
            if ((slot.SlotFlags & ent.Comp.Slots) == 0)
                continue;

            if (_inventory.TryGetSlotEntity(ent, slot.Name, out _))
                _inventory.TryUnequip(ent, slot.Name, force: true);
        }
    }

    private void OnEquipAttempt(Entity<GeneBlockedSlotsComponent> ent, ref IsEquippingTargetAttemptEvent args)
    {
        if ((args.SlotFlags & ent.Comp.Slots) != 0)
            args.Cancel();
    }
}
