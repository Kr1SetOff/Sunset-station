using Content.Shared.Damage.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Homelander;

/// <summary>
/// Marks the Homelander antagonist. Applied to whichever crew member's body AntagSelection picks
/// (see Resources/Prototypes/_Sunset/Homelander/game_rule.yml), alongside the rest of his stat
/// block (Damageable, PassiveDamage, MeleeWeapon, etc., also set directly on the AntagSelection
/// definition) - not a standalone spawnable mob like sunset-station's ghost-role version, per the
/// task's requirement that Homelander be selectable through the Antagonist menu and a GameRule.
/// </summary>
[RegisterComponent]
public sealed partial class HomelanderComponent : Component
{
    /// <summary>Damage modifier set propagated to every body part on selection.</summary>
    [DataField]
    public ProtoId<DamageModifierSetPrototype> DamageModifierSet = "Homelander";

    /// <summary>Timer for the intimidation (fear) aura.</summary>
    [ViewVariables]
    public float FearAccumulator;
}

/// <summary>
/// Lets the entity clearly hear whispers from far beyond normal earshot
/// (handled by HomelanderHearingSystem).
/// </summary>
[RegisterComponent]
public sealed partial class HyperHearingComponent : Component;
