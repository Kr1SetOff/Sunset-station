using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Tracks whether a ninjutsu practitioner still has their sneak-attack bonus available. Lost after
/// attacking someone who is aware of them (took damage from them while it was still active), regained
/// after some downtime.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NinjutsuSneakAttackComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Revealed;

    [DataField, AutoNetworkedField]
    public TimeSpan RevealedUntil = TimeSpan.Zero;

    [DataField]
    public TimeSpan RevealDuration = TimeSpan.FromSeconds(30);

    [DataField]
    public float BackstabDamageMultiplier = 1.5f;

    [DataField]
    public TimeSpan TakedownSlowdownTime = TimeSpan.FromSeconds(4);

    [DataField]
    public TimeSpan TakedownMuteTime = TimeSpan.FromSeconds(6);

    [DataField]
    public float KillMoveSpeedBonus = 1.2f;

    [DataField]
    public TimeSpan KillMoveSpeedBonusDuration = TimeSpan.FromSeconds(3);

    [DataField, AutoNetworkedField]
    public TimeSpan KillBonusUntil = TimeSpan.Zero;

    /// <summary>
    /// Flat bonus damage for the first unarmed hit while still hidden ("Assassinate").
    /// </summary>
    [DataField]
    public float AssassinateBonusDamage = 115f;

    /// <summary>
    /// Extra stamina damage dealt to a downed, already-revealed target ("Swift Strike").
    /// </summary>
    [DataField]
    public float SwiftStrikeStaminaDamage = 30f;

    /// <summary>
    /// Extra attack cooldown added when using Swift Strike, representing the halved attack speed
    /// penalty for finishing off a downed target you're no longer hidden from.
    /// </summary>
    [DataField]
    public TimeSpan SwiftStrikeExtraCooldown = TimeSpan.FromSeconds(0.5);
}
