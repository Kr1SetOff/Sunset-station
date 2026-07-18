using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.BloodWorm;

/// <summary>
/// 🌇Sunset🌇 - marks a cocoon entity that a maturing blood worm is hidden inside of (container-held,
/// invisible, non-colliding - same technique as puppeted corpses, see BloodWormHostComponent). When
/// <see cref="HatchAt"/> is reached, Content.Server._Sunset.BloodWorm.BloodWormSystem deletes the
/// cocoon and the contained worm, and spawns <see cref="NextStage"/> in its place - the worm
/// "becomes" the cocoon and then "becomes" the next stage, rather than the cocoon being a disconnected
/// decoration.
/// </summary>
[RegisterComponent]
public sealed partial class BloodWormCocoonComponent : Component
{
    public const string ContainerId = "sunset_blood_worm_cocoon";

    /// <summary>
    /// The worm hidden inside this cocoon.
    /// </summary>
    [DataField]
    public EntityUid Worm;

    /// <summary>
    /// The mob prototype that emerges when the cocoon finishes.
    /// </summary>
    [DataField]
    public EntProtoId NextStage;

    /// <summary>
    /// When the cocoon finishes and the next stage emerges.
    /// </summary>
    [DataField]
    public TimeSpan HatchAt;
}
