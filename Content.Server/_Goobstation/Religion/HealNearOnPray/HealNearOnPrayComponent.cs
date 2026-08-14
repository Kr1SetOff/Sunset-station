using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Server._Goobstation.Religion.HealNearOnPray;

/// <summary>
/// Ported from Goob-Station's Religion system. When the owning entity is prayed at (see
/// AlternatePrayableSystem), heals nearby living creatures - or damages them instead, if they're
/// unholy (Content.Shared._Starlight.Vampire.Components.UnholyComponent).
/// </summary>
[RegisterComponent]
public sealed partial class HealNearOnPrayComponent : Component
{
    [DataField]
    public DamageSpecifier Healing = new();

    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// Which sound to play on heal.
    /// </summary>
    [DataField]
    public SoundSpecifier HealSoundPath = new SoundPathSpecifier("/Audio/Effects/holy.ogg");

    /// <summary>
    /// Which sound to play on damage.
    /// </summary>
    [DataField]
    public SoundSpecifier SizzleSoundPath = new SoundPathSpecifier("/Audio/Effects/lightburn.ogg");

    /// <summary>
    /// Which effect to display on heal.
    /// </summary>
    [DataField]
    public EntProtoId HealEffect = "EffectSparks";

    /// <summary>
    /// How far around the prayed-at entity to affect creatures.
    /// </summary>
    [DataField]
    public int Range = 5;
}
