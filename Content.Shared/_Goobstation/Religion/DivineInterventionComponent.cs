using Content.Shared.Inventory;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.Religion;

/// <summary>
/// Ported from Goob-Station's Religion system. Marks an item as holy enough to shield its
/// wielder from touch-based spells (see <see cref="DivineInterventionSystem"/> and
/// SharedSpellsSystem.IsTouchSpellDenied) while held in hand, or while worn in one of
/// <see cref="ValidSpellDenialSlots"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DivineInterventionComponent : Component
{
    /// <summary>
    /// Which sound to play on spell denial.
    /// </summary>
    [DataField]
    public SoundSpecifier DenialSound = new SoundPathSpecifier("/Audio/Effects/hallelujah.ogg");

    /// <summary>
    /// Which effect to display.
    /// </summary>
    [DataField]
    public EntProtoId EffectProto = "EffectSparks";

    /// <summary>
    /// Which loc string to display.
    /// </summary>
    [DataField]
    public LocId DenialString = "nullrod-spelldenial-popup";

    /// <summary>
    /// Valid inventory slots for spell denial when equipped (rather than held).
    /// </summary>
    [DataField]
    public SlotFlags ValidSpellDenialSlots = SlotFlags.NONE;
}
