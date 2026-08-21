using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Slasher.Components;

/// <summary>
/// Applied to the machete to provide cumulative soul steal bonuses.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherSoulStealMacheteBonusComponent : Component
{
    [DataField]
    public float SlashBonus;
}
