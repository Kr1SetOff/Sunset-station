using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Krav Maga practitioner state (ported from Goob Station / mini-station-goob). The style is
/// action-based rather than combo-based: the user primes one of three techniques with an action,
/// and their next unarmed strike applies it.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class KravMagaComponent : Component
{
    [DataField, AutoNetworkedField]
    public KravMagaMove? SelectedMove;

    /// <summary>
    /// The granted action entities, removed again when the style is revoked.
    /// </summary>
    [DataField]
    public List<EntityUid> Actions = new();
}

/// <summary>
/// Marks a Krav Maga action entity with which technique it primes and its parameters.
/// </summary>
[RegisterComponent]
public sealed partial class KravMagaActionComponent : Component
{
    [DataField]
    public KravMagaMove Move;

    [DataField]
    public float StaminaDamage;

    /// <summary>
    /// Duration in seconds of the technique's status effect (mute / blocked breathing).
    /// </summary>
    [DataField]
    public float EffectTime;
}

/// <summary>
/// The target's breathing is blocked by a Krav Maga lung punch until <see cref="ExpiresAt"/>.
/// Enforced by RespiratorSystem; expiry is handled by SharedMartialArtsSystem.Update.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BreathingBlockedComponent : Component
{
    [DataField]
    public TimeSpan ExpiresAt;
}

public enum KravMagaMove : byte
{
    LegSweep,
    NeckChop,
    LungPunch,
}
