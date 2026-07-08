using Content.Shared._Starlight.Medical.Body.Part;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Autosurgeon;

/// <summary>
/// A single-use cybernetic upgrade device. Ported from Goobstation/Reserve-Station's Autosurgeon,
/// adapted to this fork's own body system: swaps a body part (e.g. an arm) for a cyberlimb, or an
/// organ (e.g. the heart) for a replacement, instead of a flat stat bonus. The removed original part
/// is placed in the user's free hand, or left on the ground if their hands are full.
/// </summary>
[RegisterComponent]
public sealed partial class AutosurgeonComponent : Component
{
    [DataField]
    public TimeSpan DoAfterTime = TimeSpan.FromSeconds(15);

    [DataField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_Goobstation/Machines/autosurgeon.ogg");

    [DataField]
    public bool Used;

    /// <summary>
    /// Body part type to replace (e.g. Arm). Leave null to use the organ-replacement path instead.
    /// </summary>
    [DataField]
    public BodyPartType? TargetPartType;

    [DataField]
    public BodyPartSymmetry TargetSymmetry = BodyPartSymmetry.None;

    /// <summary>
    /// Entity prototype to attach in place of the removed body part.
    /// </summary>
    [DataField]
    public EntProtoId? NewPartProto;

    /// <summary>
    /// Organ slot id on the torso to replace (e.g. "heart"). Leave null to use the body-part path instead.
    /// </summary>
    [DataField]
    public string? TargetOrganSlot;

    /// <summary>
    /// Entity prototype to insert in place of the removed organ.
    /// </summary>
    [DataField]
    public EntProtoId? NewOrganProto;

    /// <summary>
    /// If set, permanently multiplies the user's move speed by this factor once the swap succeeds
    /// (used by the Sandevistan heart - this fork's organs don't support Goobstation's onAdd effect
    /// hook, so the bonus is applied directly here instead of living on the organ itself).
    /// </summary>
    [DataField]
    public float? MoveSpeedMultiplier;

    /// <summary>
    /// Locale id for the popup shown to the user once the upgrade completes.
    /// </summary>
    [DataField]
    public string SuccessPopup = "autosurgeon-success";

    /// <summary>
    /// Locale id for the popup shown if the operation couldn't find what it needed to replace.
    /// </summary>
    [DataField]
    public string FailurePopup = "autosurgeon-failure";
}
