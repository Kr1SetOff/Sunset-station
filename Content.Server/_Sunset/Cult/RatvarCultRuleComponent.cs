namespace Content.Server._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - round-level state for the RatvarCult game rule. Headcount/round-start selection is
/// entirely handled by the generic AntagSelection component (see roundstart.yml) - this tracks whether
/// the cult actually pulled off the summoning rite (round-end summary + the leader's objective) and how
/// many crew have been converted so far (the shared conversion objective), both read live by
/// Content.Server._Sunset.Cult.RatvarCultObjectiveSystem instead of being duplicated onto every mind.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarCultRuleComponent : Component
{
    [DataField]
    public bool RatvarSummoned;

    [DataField]
    public int ConvertedCount;

    /// <summary>
    /// Total sacrifices carried out via the Rune of Offering (see RatvarCultSystem.CompleteConvertRite) -
    /// porting tg's GLOB.sacrificed. Spent 3-at-a-time to revive a dead cultist via the Raise Dead rune.
    /// </summary>
    [DataField]
    public int SacrificeCount;

    [DataField]
    public int SacrificesUsed;
}
