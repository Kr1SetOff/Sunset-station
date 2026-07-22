namespace Content.Server._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - round-level state for the RatvarServant game rule (the real Servants of Ratvar
/// antagonist - see Content.Shared._Sunset.Ratvar.RatvarServantComponent for why this is a separate
/// system from Content.Server._Sunset.Cult). Headcount/round-start selection is handled by the generic
/// AntagSelection component; this tracks how many crew have been converted so far.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarServantRuleComponent : Component
{
    [DataField]
    public int ConvertedCount;

    /// <summary>
    /// The cult's shared energy pool, fed by Herald's Beacons (and later Integration Cogs). Porting
    /// the ss220.space RU wiki's pooled "энергия" resource used for the final ritual.
    /// </summary>
    [DataField]
    public float Energy;

    /// <summary>
    /// Set once the final ritual completes and Ratvar is summoned (round-end summary + win condition).
    /// </summary>
    [DataField]
    public bool RatvarSummoned;

    /// <summary>
    /// Constructs (Cogscarab/Marauder/Brass Golem) animated so far and still alive - capped at
    /// <see cref="MaxConstructs"/> so brass/energy farming can't field an unlimited army.
    /// </summary>
    [DataField]
    public List<EntityUid> ActiveConstructs = new();

    public const int MaxConstructs = 6;
}
