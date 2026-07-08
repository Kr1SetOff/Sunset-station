namespace Content.Shared._Sunset.Spy;

/// <summary>
/// Marks a worn item that other players can't strip off its wearer - see CovertClothingSystem,
/// which cancels StripAttemptEvent for any item carrying this. Unlike SelfUnremovableClothingComponent
/// (blocks the WEARER from removing their own item), this only blocks OTHERS; the wearer can still
/// take it off normally through their own inventory.
/// </summary>
[RegisterComponent]
public sealed partial class CovertClothingComponent : Component;
