using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.TheBoys.Components;

/// <summary>
/// Marks whichever crew member The Boys gamerule picked as Butcher - lets TheBoysRuleSystem tell
/// him apart from the other four team members when AfterAntagEntitySelectedEvent fires, since that
/// event is raised on the rule entity (see TheBoysTeamRuleComponent), not the picked player.
/// Networked (like TheBoysTeammateComponent) so the client-side name overlay can show his codename
/// to the rest of the team (see Content.Client._Sunset.TheBoys.TheBoysNameOverlay).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TheBoysButcherComponent : Component;
