namespace Content.Shared._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - Herald's Beacon, porting the ss220.space RU wiki's description: a brass structure that
/// passively generates energy for the cult's shared pool and heals nearby servants over time.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarHeraldsBeaconComponent : Component
{
    [DataField]
    public float Range = 3f;

    [DataField]
    public TimeSpan TickInterval = TimeSpan.FromSeconds(1);

    [DataField]
    public TimeSpan NextTick;

    /// <summary>
    /// Energy added to the cult's shared pool each tick (RU wiki: 2.5/tick).
    /// </summary>
    [DataField]
    public float EnergyPerTick = 2.5f;

    /// <summary>
    /// Brute/burn healed on nearby servants each tick (RU wiki: 10/10).
    /// </summary>
    [DataField]
    public float HealPerTick = 10f;
}
