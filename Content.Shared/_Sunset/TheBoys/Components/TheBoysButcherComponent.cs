namespace Content.Shared._Sunset.TheBoys.Components;

/// <summary>
/// Marks whichever crew member The Boys gamerule picked as Butcher - lets TheBoysRuleSystem tell
/// him apart from the other four team members when AfterAntagEntitySelectedEvent fires, since that
/// event is raised on the rule entity (see TheBoysTeamRuleComponent), not the picked player.
/// </summary>
[RegisterComponent]
public sealed partial class TheBoysButcherComponent : Component;
