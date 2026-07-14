namespace Content.Server._Sunset.TheBoys.Components;

/// <summary>
/// Marker for The Boys' team (Butcher + Frenchie/Hughie/Mother's Milk/Kimiko) GameRule - separate
/// from TheBoysHomelanderRule so admins can force/track/remove either half of the mode independently
/// (see game_presets.yml's TheBoys preset, which runs both rules together).
/// </summary>
[RegisterComponent]
public sealed partial class TheBoysTeamRuleComponent : Component
{
    /// <summary>
    /// The body TheBoysHomelanderRule picked this round, stamped here by AssignTeam before the team
    /// pick runs - both so the team pool excludes him, and so Butcher's "eliminate Homelander"
    /// objective (see OnMemberSelected) has someone to target without needing to reach across to a
    /// separate rule entity.
    /// </summary>
    [DataField]
    public EntityUid? HomelanderBody;
}
