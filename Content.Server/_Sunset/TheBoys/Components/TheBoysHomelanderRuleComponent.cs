namespace Content.Server._Sunset.TheBoys.Components;

/// <summary>
/// Marker for The Boys' Homelander pick - a separate GameRule from TheBoysTeamRule so the two can be
/// combined (or independently forced/removed) via the gamePreset's rules list. Hand-picked directly
/// by TheBoysRuleSystem on RulePlayerJobsAssignedEvent, same as the team - not the standard
/// preference-pool selection the standalone Homelander GameRule uses, since nobody could plausibly
/// have pre-opted into a preference for this gamemode-exclusive pick.
/// </summary>
[RegisterComponent]
public sealed partial class TheBoysHomelanderRuleComponent : Component;
