namespace Content.Server._Sunset.Homelander;

/// <summary>
/// Marker for the Homelander GameRule. All the actual work happens through the generic
/// AntagSelection machinery (see Resources/Prototypes/_Sunset/Homelander/game_rule.yml) and
/// HomelanderSystem's AfterAntagEntitySelectedEvent handler.
/// </summary>
[RegisterComponent]
public sealed partial class HomelanderRuleComponent : Component;
