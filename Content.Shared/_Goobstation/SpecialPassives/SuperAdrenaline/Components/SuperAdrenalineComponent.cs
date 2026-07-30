using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.SpecialPassives.SuperAdrenaline.Components;

/// <summary>
///     Entities with this shrug off stun/knockdown/sleep and regenerate stamina rapidly.
///     Simplified from Goob-Station's version: instead of intercepting "before stun/knockdown"
///     attempt events (which this fork doesn't have), it reactively clears the status each tick -
///     this fork has no pain/consciousness system to grant true immunity against either.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SuperAdrenalineComponent : Component
{
    public ProtoId<AlertPrototype>? AlertId;

    [DataField]
    public float? Duration;

    public TimeSpan MaxDuration = TimeSpan.Zero;

    public TimeSpan UpdateTimer = default!;

    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    [DataField]
    public bool IgnoreKnockdown = true;

    [DataField]
    public bool IgnoreStun = true;

    [DataField]
    public bool IgnoreSleep = true;

    [DataField]
    public float StaminaRegeneration = 10f;

    [DataField]
    public DamageSpecifier? PassiveDamage;
}
