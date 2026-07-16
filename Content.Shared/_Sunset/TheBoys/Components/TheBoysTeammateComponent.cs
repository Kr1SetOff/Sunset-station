using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.TheBoys.Components;

/// <summary>
/// Marks Frenchie/Hughie/Mother's Milk/Kimiko - the four rank-and-file members of Butcher's team
/// who only get a flavor "you're on a team with Butcher" briefing (see TheBoysRuleSystem), not a
/// real mechanical objective like Butcher gets. Networked so the client-side name overlay can show
/// this teammate's codename to the rest of the team (see Content.Client._Sunset.TheBoys.TheBoysNameOverlay),
/// which reads the character's TheBoysHughie/Frenchie/MothersMilk/Kimiko tag to pick the label.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TheBoysTeammateComponent : Component;
