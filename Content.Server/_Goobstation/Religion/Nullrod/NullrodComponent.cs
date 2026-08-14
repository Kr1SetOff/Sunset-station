using Content.Shared.Damage;
using Robust.Shared.Audio;

namespace Content.Server._Goobstation.Religion.Nullrod;

/// <summary>
/// Ported from Goob-Station's Religion system. Marks a holy weapon that punishes anyone without
/// BibleUserComponent for swinging it, and that a chaplain can bind to themselves via a verb (see
/// NullrodSystem) so it can be summoned back to their hand later at an altar (RecallNullrodSystem).
/// </summary>
[RegisterComponent]
public sealed partial class NullrodComponent : Component
{
    /// <summary>
    /// Whether non bible-users are punished for attacking with this weapon.
    /// </summary>
    [DataField]
    public bool UntrainedUseRestriction = true;

    /// <summary>
    /// How much damage is dealt to an untrained user attempting to attack with this.
    /// </summary>
    [DataField(required: true)]
    public DamageSpecifier DamageOnUntrainedUse = default!;

    /// <summary>
    /// Which loc string to pop up on the untrained user.
    /// </summary>
    [DataField(required: true)]
    public LocId UntrainedUseString;

    /// <summary>
    /// Which sound to play on untrained use.
    /// </summary>
    [DataField]
    public SoundSpecifier UntrainedUseSound = new SoundPathSpecifier("/Audio/Effects/hallelujah.ogg");

    /// <summary>
    /// Don't spam the popup/damage every attack attempt against the same target.
    /// </summary>
    [DataField]
    public TimeSpan PopupCooldown = TimeSpan.FromSeconds(3.0);

    [DataField]
    public TimeSpan NextPopupTime;

    /// <summary>
    /// Whether this nullrod can be bound to a chaplain and recalled at an altar.
    /// </summary>
    [DataField]
    public bool Recallable = true;
}
