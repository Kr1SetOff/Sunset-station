namespace Content.Shared._Sunset.MalfAi;

/// <summary>
/// 🌇Sunset🌇 - marks a station AI brain as a Malfunctioning AI (the SS13 "malf" antagonist).
/// Holds the tg-station-style economy knobs (CPU income, APC hacking, doomsday gating) consumed by
/// Content.Server._Sunset.MalfAi.MalfAiSystem. Numbers mirror tg: 50 starting CPU (set by the rule
/// system when the store is created), ~10 CPU/minute passive income, +10 CPU per hacked APC plus a
/// small permanent income bump, and the Doomsday Device gated behind 10 hacked APCs.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiComponent : Component
{
    /// <summary>
    /// How many APCs this AI has successfully hacked so far.
    /// </summary>
    [DataField]
    public int HackedApcs;

    /// <summary>
    /// Base passive CPU income per minute, before the per-hacked-APC bonus.
    /// </summary>
    [DataField]
    public float PassiveCpuPerMinute = 10f;

    /// <summary>
    /// Extra passive CPU per minute for each hacked APC.
    /// </summary>
    [DataField]
    public float CpuPerHackedApcPerMinute = 1f;

    /// <summary>
    /// One-time CPU payout for hacking an APC (tg wiki: "an additional 10 for every APC you hack").
    /// </summary>
    [DataField]
    public float CpuOnHack = 10f;

    /// <summary>
    /// How long hacking a single APC takes.
    /// </summary>
    [DataField]
    public TimeSpan HackDuration = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Hacked APCs required before the Doomsday Device can be activated ("So you cant speedrun delta").
    /// </summary>
    [DataField]
    public int DoomsdayRequiredApcs = 10;

    /// <summary>
    /// Countdown from doomsday activation to detonation. tg: 450 seconds.
    /// </summary>
    [DataField]
    public TimeSpan DoomsdayDelay = TimeSpan.FromSeconds(450);

    /// <summary>
    /// How long a Hostile Station Lockdown holds the bolts before releasing. tg: 90 seconds.
    /// </summary>
    [DataField]
    public TimeSpan LockdownDuration = TimeSpan.FromSeconds(90);

    /// <summary>
    /// Chance for each light on the station to blow when Blackout is used. tg: 30%.
    /// </summary>
    [DataField]
    public float BlackoutBreakChance = 0.3f;

    // Server-side running state (not saved/networked - a mid-round malf is always set up by the rule system).

    /// <summary>
    /// When the next passive CPU income tick happens.
    /// </summary>
    public TimeSpan NextCpuTick;

    /// <summary>
    /// Whether the doomsday device has been armed. One-way unless the AI dies or is carded.
    /// </summary>
    public bool DoomsdayActive;

    /// <summary>
    /// When the armed doomsday device detonates.
    /// </summary>
    public TimeSpan DoomsdayEndTime;

    /// <summary>
    /// The lowest countdown warning mark (in seconds remaining) already announced for the active
    /// doomsday, so each mark only plays once.
    /// </summary>
    public int DoomsdayLastWarning = int.MaxValue;

    /// <summary>
    /// The station the doomsday device was armed against.
    /// </summary>
    public EntityUid? DoomsdayStation;

    /// <summary>
    /// When an active Hostile Station Lockdown releases, if one is running.
    /// </summary>
    public TimeSpan? LockdownEndTime;

    /// <summary>
    /// Doors bolted by the current lockdown, to unbolt when it expires.
    /// </summary>
    public List<EntityUid> LockdownDoors = new();
}
