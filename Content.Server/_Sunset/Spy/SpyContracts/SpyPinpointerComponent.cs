namespace Content.Server._Sunset.Spy.SpyContracts;

/// <summary>
/// Marks the Spy Pinpointer item so SpyContractSystem can reliably find it in the spy's
/// inventory to point at the active contract's target.
/// </summary>
[RegisterComponent]
public sealed partial class SpyPinpointerComponent : Component;
