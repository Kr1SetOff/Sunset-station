using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.TheBoys.Components;

/// <summary>
/// Marks Frenchie/Hughie/Mother's Milk/Kimiko - the four rank-and-file members of Butcher's team
/// who only get a flavor "you're on a team with Butcher" briefing (see TheBoysRuleSystem), not a
/// real mechanical objective like Butcher gets. Networked so the client-side status icon system can
/// show StatusIcon to the rest of the team - see the faction icon's showTo list in
/// Resources/Prototypes/_Sunset/TheBoys/status_icon.yml.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TheBoysTeammateComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "TheBoysTeamFaction";
}
