using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._Sunset.Sandevistan;

/// <summary>
/// Ported from Reserve-Station/Goobstation's Sandevistan cyberware (Content.Goobstation.Shared.
/// Sandevistan.SandevistanUserComponent). Includes the afterimage trail and screen-glitch shader;
/// the slowfield only affects mobs (not projectiles/thrown items - this fork has no equivalent
/// ranged "ammo shot" event to hook the same way, and no DogVision component). The core loop:
/// toggle on for a movement/attack-speed boost, a load meter that fills while active and drains
/// while off, and escalating debuffs (jitter, stamina loss, damage, knockdown, forced shutdown,
/// death) as load crosses each threshold.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SandevistanUserComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string? ToggleAction = "ActionToggleSandevistan";

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleActionEntity;

    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField]
    public TimeSpan PopupDelay = TimeSpan.FromSeconds(3);

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextPopupTime = TimeSpan.Zero;

    /// <summary>Current overload load - only changes while Active (or draining back down while off).</summary>
    [DataField, AutoNetworkedField]
    public float CurrentLoad;

    [DataField]
    public float LoadPerActiveSecond = 1f;

    [DataField]
    public float LoadPerInactiveSecond = -0.25f;

    [DataField, AutoNetworkedField]
    public Dictionary<SandevistanState, FixedPoint2> Thresholds = new();

    [DataField]
    public Dictionary<SandevistanState, SandevistanEffect[]> Effects = new()
    {
        { SandevistanState.Shaking, [new SandevistanJitterEffect()] },
        { SandevistanState.Stamina, [new SandevistanStaminaDamageEffect()] },
        { SandevistanState.Damage, [new SandevistanDamageEffect()] },
        { SandevistanState.Knockdown, [new SandevistanKnockdownEffect()] },
        { SandevistanState.Disable, [new SandevistanDisableEffect()] },
        { SandevistanState.Death, [new SandevistanDeathEffect()] },
    };

    [DataField, AutoNetworkedField]
    public float MovementSpeedModifier = 2f;

    [DataField, AutoNetworkedField]
    public float AttackSpeedModifier = 2f;

    /// <summary>Cycles through the rainbow for each afterimage's tint - see SandevistanSystem.SpawnAfterimage.</summary>
    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int ColorAccumulator;

    [DataField]
    public float AfterimageInterval = 0.08f;

    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoNetworkedField]
    public TimeSpan NextAfterimageTime = TimeSpan.Zero;

    [DataField]
    public SoundSpecifier? StartSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_start.ogg");

    [DataField]
    public SoundSpecifier? EndSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_end.ogg");

    [DataField]
    public SoundSpecifier? LoopSound = new SoundPathSpecifier("/Audio/_Goobstation/Misc/sande_loop.ogg")
    {
        Params = new AudioParams { Loop = true },
    };

    [DataField]
    public float LoopSoundDelay = 2.5f;

    [DataField]
    public EntityUid? PlayingStream;

    /// <summary>Alert shown while active, so the user can see how close they are to overloading.</summary>
    [DataField, AutoNetworkedField]
    public ProtoId<AlertPrototype> LoadAlert = "SandevistanLoad";

    #region Slowfield

    /// <summary>
    /// Slows down nearby mobs while active - a stripped-down version of the source's slowfield
    /// (mobs only, no projectiles/thrown items).
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool SlowfieldEnabled;

    [DataField]
    public float SlowfieldRadius = 7f;

    [DataField]
    public float MobSpeedMultiplier = 0.15f;

    #endregion
}
