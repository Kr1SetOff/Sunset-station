using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.TheBoys.Components;

/// <summary>
/// Marks whichever crew member The Boys gamerule picked as Butcher - lets TheBoysRuleSystem tell
/// him apart from the other four team members when AfterAntagEntitySelectedEvent fires, since that
/// event is raised on the rule entity (see TheBoysTeamRuleComponent), not the picked player.
/// Networked (like TheBoysTeammateComponent) so the client-side status icon system can show
/// StatusIcon to the rest of the team - see the faction icon's showTo list in
/// Resources/Prototypes/_Sunset/TheBoys/status_icon.yml.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TheBoysButcherComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FactionIconPrototype> StatusIcon { get; set; } = "TheBoysTeamFaction";
}
