using Content.Shared.Damage;
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

    /// <summary>True while the twin eye-laser channel (see HomelanderHeatVisionEvent) is active.</summary>
    [ViewVariables]
    public bool LaserActive;

    /// <summary>When the current laser channel auto-ends.</summary>
    [ViewVariables]
    public TimeSpan LaserEndTime;

    /// <summary>Accumulates frame time between laser damage ticks.</summary>
    [ViewVariables]
    public float LaserTickAccumulator;

    /// <summary>
    /// Accumulates frame time between laser visual refreshes - kept separate and slower than the
    /// damage tick so each hitscan bolt's own travel animation finishes before the next one spawns,
    /// instead of a dozen overlapping flashes stacking up into visual noise.
    /// </summary>
    [ViewVariables]
    public float LaserVisualAccumulator;

    /// <summary>Per-second damage of the current laser channel (copied from the action event at activation).</summary>
    [ViewVariables]
    public DamageSpecifier LaserDamagePerSecond = new();

    /// <summary>Max beam range of the current laser channel.</summary>
    [ViewVariables]
    public float LaserRange;

    /// <summary>Lateral eye separation of the current laser channel.</summary>
    [ViewVariables]
    public float LaserEyeOffset;

    /// <summary>Cooldown applied to the action once the current laser channel ends (whether it
    /// ran out its duration or was toggled off early) - copied from the action event at activation.</summary>
    [ViewVariables]
    public TimeSpan LaserLockout;

    /// <summary>The heat vision action entity, so EndLaser can put it on cooldown once the channel
    /// actually stops, regardless of whether that's from timing out or a manual toggle-off.</summary>
    [ViewVariables]
    public EntityUid? LaserActionEntity;

    /// <summary>The looping laser sound's own audio entity, so EndLaser can stop it.</summary>
    [ViewVariables]
    public EntityUid? LaserSoundEntity;

    /// <summary>
    /// True once his thermal vision action has had its icon swapped to the custom sprite - checked
    /// once a tick until it succeeds, see HomelanderSystem.TryRetintThermalVision.
    /// </summary>
    [ViewVariables]
    public bool ThermalIconApplied;
}

/// <summary>
/// Lets the entity clearly hear whispers from far beyond normal earshot
/// (handled by HomelanderHearingSystem).
/// </summary>
[RegisterComponent]
public sealed partial class HyperHearingComponent : Component;
