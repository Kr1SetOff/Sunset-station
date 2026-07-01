using Content.Shared.FixedPoint;

namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Tracks Capoeira's short-lived combat buffs: the flat damage/cooldown bonus gained from missing a
/// strike, and the attack-speed bonus gained from a completed combo.
/// </summary>
[RegisterComponent]
public sealed partial class CapoeiraMomentumComponent : Component
{
    [DataField]
    public FixedPoint2 MissDamageBonus = 2;

    [DataField]
    public TimeSpan MissBonusUntil = TimeSpan.Zero;

    [DataField]
    public float AttackSpeedBonus = 1.5f;

    [DataField]
    public TimeSpan AttackSpeedBonusUntil = TimeSpan.Zero;
}
