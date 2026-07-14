using Content.Shared.Actions;
using Content.Shared.Damage;

namespace Content.Shared._Sunset.TheBoys;

/// <summary>
/// Instant action: starts Butcher's twin-laser channel while his Compound V power is active - see
/// HomelanderHeatVisionEvent (Content.Shared._Sunset.Homelander), which this deliberately mirrors
/// field-for-field. Kept as a separate type (rather than reusing HomelanderHeatVisionEvent directly)
/// so TheBoysPowersSystem's own broadcast subscription doesn't collide with HomelanderSystem's -
/// each system only ever wants to hear about its own performer's channel.
/// </summary>
public sealed partial class TheBoysButcherLaserEyesEvent : InstantActionEvent
{
    /// <summary>Maximum length of the twin-laser channel before it ends automatically.</summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(15);

    /// <summary>How long the action stays on cooldown after the channel ends.</summary>
    [DataField]
    public TimeSpan Lockout = TimeSpan.FromSeconds(20);

    /// <summary>Damage dealt per second to whatever each beam is hitting.</summary>
    [DataField]
    public DamageSpecifier DamagePerSecond = new();

    /// <summary>Maximum beam length, in tiles.</summary>
    [DataField]
    public float Range = 12f;

    /// <summary>Lateral offset of each eye from center, in tiles - controls how far apart the two beams are.</summary>
    [DataField]
    public float EyeOffset = 0.06f;
}
