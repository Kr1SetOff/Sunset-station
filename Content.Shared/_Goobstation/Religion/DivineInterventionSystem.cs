using System.Diagnostics.CodeAnalysis;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._Goobstation.Religion;

/// <summary>
/// Ported from Goob-Station's Religion system (adapted: this fork's touch-spell hook is a plain
/// method - Content.Shared._Goobstation.Wizard.SharedSpellsSystem.IsTouchSpellDenied - rather than
/// Goob's event-relay setup, so this is a much smaller trimmed-down port that only keeps the
/// "does this target have a holy item on them" check and its popup/sound/effect flavor).
/// </summary>
public sealed class DivineInterventionSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;

    /// <summary>
    /// Whether a touch-based spell against this target should be denied because they're holding
    /// or wearing a holy item, and plays the denial flavor (popup/sound/effect) if so.
    /// </summary>
    public bool ShouldDeny(EntityUid target)
    {
        if (!TryFindDenyingItem(target, out var denyingItem))
            return false;

        DenialEffects(denyingItem.Value, target);
        return true;
    }

    private bool TryFindDenyingItem(EntityUid target, [NotNullWhen(true)] out EntityUid? denyingItem)
    {
        denyingItem = null;
        var divineQuery = GetEntityQuery<DivineInterventionComponent>();

        foreach (var held in _hands.EnumerateHeld(target))
        {
            if (!divineQuery.HasComponent(held))
                continue;

            denyingItem = held;
            return true;
        }

        var slots = _inventory.GetSlotEnumerator(target, SlotFlags.WITHOUT_POCKET);
        while (slots.NextItem(out var item, out var slot))
        {
            if (!divineQuery.TryGetComponent(item, out var comp))
                continue;

            if ((slot.SlotFlags & comp.ValidSpellDenialSlots) == 0x0)
                continue;

            denyingItem = item;
            return true;
        }

        return false;
    }

    private void DenialEffects(EntityUid item, EntityUid target)
    {
        if (!TryComp<DivineInterventionComponent>(item, out var comp))
            return;

        _popup.PopupPredicted(Loc.GetString(comp.DenialString), target, target, PopupType.MediumCaution);

        if (_net.IsClient)
            return;

        Spawn(comp.EffectProto, Transform(target).Coordinates);
        _audio.PlayPvs(comp.DenialSound, target);
    }
}
