namespace Content.Shared._Sunset.TheBoys.Components;

/// <summary>
/// Marks Butcher's personal crowbar (see Resources/Prototypes/_Sunset/TheBoys/items.yml) - deals
/// normal crowbar damage to everything, except a Homelander antagonist specifically, who takes 3x
/// (see TheBoysRuleSystem.OnHomelanderAttacked).
/// </summary>
[RegisterComponent]
public sealed partial class ButcherCrowbarComponent : Component;
