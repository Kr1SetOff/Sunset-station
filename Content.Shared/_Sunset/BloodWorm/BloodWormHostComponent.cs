namespace Content.Shared._Sunset.BloodWorm;

/// <summary>
/// 🌇Sunset🌇 - marks a humanoid corpse currently puppeted by a blood worm. The worm entity sits in
/// a container inside the body, and the worm player's mind is transferred into the body until they
/// leave (or the body runs out of blood). Body death no longer auto-ejects the worm (see
/// BloodWormSystem) - Revive Host lets the worm try to save a host that died in combat instead.
/// </summary>
[RegisterComponent]
public sealed partial class BloodWormHostComponent : Component
{
    public const string ContainerId = "sunset_blood_worm";

    /// <summary>
    /// The worm hiding inside this body.
    /// </summary>
    [DataField]
    public EntityUid? Worm;

    /// <summary>
    /// The Leave Host action entity granted to the puppeted body.
    /// </summary>
    [DataField]
    public EntityUid? LeaveAction;

    /// <summary>
    /// The Inject Blood action entity granted to the puppeted body.
    /// </summary>
    [DataField]
    public EntityUid? InjectAction;

    /// <summary>
    /// The Revive Host action entity granted to the puppeted body.
    /// </summary>
    [DataField]
    public EntityUid? ReviveAction;
}
