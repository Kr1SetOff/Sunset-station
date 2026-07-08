namespace Content.Server._Sunset.Spy.SpyContracts;

/// <summary>
/// Marks the Spy Tracker item - a dual-purpose gadget (surveillance bug / sabotage tool) used to
/// work a contract's target. Application logic lives in SpyContractSystem via AfterInteractEvent.
/// </summary>
[RegisterComponent]
public sealed partial class SpyTrackerComponent : Component;
