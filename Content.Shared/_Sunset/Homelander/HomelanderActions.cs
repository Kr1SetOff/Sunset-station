using Content.Shared.Actions;
using Content.Shared.Damage;

namespace Content.Shared._Sunset.Homelander;

/// <summary>
/// Instant action: burns a line in front of Homelander in the direction he's facing, dealing heat
/// damage to mobs and structural damage to walls caught in it. Simplified from sunset-station's
/// cursor-following channelled beam (which relies on the heretic StarGaze cursor/visual system this
/// fork doesn't have) into a single instant burst - same "heat vision" identity, no missing deps.
/// Handled server-side by HomelanderSystem.
/// </summary>
public sealed partial class HomelanderHeatVisionEvent : InstantActionEvent
{
    /// <summary>Damage applied to each mob caught in the line.</summary>
    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>Line length, in tiles.</summary>
    [DataField]
    public float Range = 10f;
}

/// <summary>
/// Instant action: an intimidating scream that knocks nearby crew off their feet (knockdown, no
/// stun). Handled server-side by HomelanderSystem.
/// </summary>
public sealed partial class HomelanderScreamEvent : InstantActionEvent
{
    /// <summary>Radius affected.</summary>
    [DataField]
    public float Range = 5f;

    /// <summary>How long victims stay knocked down, in seconds.</summary>
    [DataField]
    public float KnockdownSeconds = 3f;
}

/// <summary>
/// Instant action: a ground-pound shockwave that shoves everything nearby away and knocks down
/// anyone caught in it. Replaces sunset-station's flight-landing-triggered shockwave (this fork has
/// no Flight system) with a directly-triggerable ability instead.
/// Handled server-side by HomelanderSystem.
/// </summary>
public sealed partial class HomelanderShockwaveEvent : InstantActionEvent
{
    [DataField]
    public float Range = 5f;

    [DataField]
    public float Force = 15f;

    [DataField]
    public float KnockdownSeconds = 2f;
}
