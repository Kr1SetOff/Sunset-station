namespace Content.Server._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - objective condition for "convert N crew". No per-mind data needed - progress is read
/// live off the round's RatvarCultRuleComponent.ConvertedCount by RatvarCultObjectiveSystem, instead of
/// duplicating a running total onto every cultist's own objective like Cosmic Cult's conversion
/// objective does.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarCultConvertConditionComponent : Component;

/// <summary>
/// 🌇Sunset🌇 - objective condition for "complete the summoning rite" (head cultist only, see
/// RatvarCultSystem.AfterAntagSelected). Reads RatvarCultRuleComponent.RatvarSummoned live.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarCultSummonConditionComponent : Component;
