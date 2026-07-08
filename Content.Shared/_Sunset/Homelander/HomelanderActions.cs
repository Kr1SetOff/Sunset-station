using Content.Shared.Actions;
using Content.Shared.Damage;

namespace Content.Shared._Sunset.Homelander;

/// <summary>
/// Instant action: starts a twin-laser channel, up to Duration long. While channeling, Homelander's
/// body continuously rotates to face the mouse cursor (MouseRotatorComponent) and HomelanderSystem
/// fires two parallel beams - one per eye - in whatever direction he's currently facing, dealing
/// damage on a tick interval. Pressing the action again while channeling ends it early (toggle
/// off); otherwise it ends automatically once Duration elapses. Either way, the action's cooldown
/// is only applied once the channel actually stops (see HomelanderSystem.EndLaser), so the button
/// stays clickable - and therefore able to toggle off - for the whole channel.
/// </summary>
public sealed partial class HomelanderHeatVisionEvent : InstantActionEvent
{
    /// <summary>Maximum length of the twin-laser channel before it ends automatically.</summary>
    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(25);

    /// <summary>How long the action stays on cooldown after the channel ends.</summary>
    [DataField]
    public TimeSpan Lockout = TimeSpan.FromSeconds(30);

    /// <summary>Damage dealt per second to whatever each beam is hitting.</summary>
    [DataField]
    public DamageSpecifier DamagePerSecond = new();

    /// <summary>Maximum beam length, in tiles.</summary>
    [DataField]
    public float Range = 15f;

    /// <summary>Lateral offset of each eye from center, in tiles - controls how far apart the two beams are.</summary>
    [DataField]
    public float EyeOffset = 0.06f;
}
