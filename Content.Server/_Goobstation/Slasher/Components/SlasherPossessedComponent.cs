using Robust.Shared.Audio;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Slasher.Components;

/// <summary>
/// Added to a victim while the Slasher is riding their body.
/// Sunset-specific: Goob-Station has a generic Devil-driven PossessionSystem that this fork does not,
/// so this is a trimmed-down, slasher-only version of the same mechanic (mind swap on a timer).
/// </summary>
[RegisterComponent]
public sealed partial class SlasherPossessedComponent : Component
{
    [ViewVariables]
    public EntityUid OriginalMindId;

    [ViewVariables]
    public EntityUid PossessorMindId;

    [ViewVariables]
    public EntityUid PossessorOriginalEntity;

    [ViewVariables]
    public TimeSpan PossessionEndTime;

    [ViewVariables]
    public TimeSpan PossessionTimeRemaining;

    /// <summary>
    /// Nullspace container holding the dummy entity that parks the victim's mind for the duration.
    /// </summary>
    [ViewVariables]
    public Container? PossessedContainer;

    [ViewVariables]
    public EntityUid? DummyEntity;

    [DataField]
    public EntProtoId EndPossessionAction = "ActionSlasherEndPossession";

    [ViewVariables]
    public EntityUid? ActionEntity;

    [DataField]
    public SoundSpecifier PossessionSound = new SoundPathSpecifier("/Audio/_Goobstation/Effects/bone_crack.ogg");
}
