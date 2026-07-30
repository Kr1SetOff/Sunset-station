using Content.Shared.Alert;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.SpecialPassives.Fleshmend.Components;

/// <summary>
///     Entities with this rapidly heal physical injuries. Simplified from Goob-Station's version,
///     which relies on Shitmed's wound/consciousness systems that don't exist in this fork.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class FleshmendComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public float MovementSpeedDebuff = 0.75f;

    public ProtoId<AlertPrototype>? AlertId;

    [DataField]
    public float? Duration;

    public TimeSpan MaxDuration = TimeSpan.Zero;

    public TimeSpan UpdateTimer = default!;

    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    public EntityUid? SoundSource;

    [DataField]
    public SoundSpecifier? PassiveSound;

    [DataField]
    public ResPath ResPath;

    [DataField]
    public string? EffectState;

    [DataField]
    public float BruteHeal = -6f;

    [DataField]
    public float BurnHeal = -4f;

    [DataField]
    public float AsphyxHeal = -3f;

    [DataField]
    public float BleedingAdjust = -2.5f;

    [DataField]
    public float BloodLevelAdjust = 10f;
}
