namespace Content.Shared._Sunset.Spy;

/// <summary>
/// Put on a clothing item (the Spy Uplink's thermal goggles) to grant the wearer ThermalVision
/// while it's equipped in its proper slot - the same clothes-grant pattern as
/// ClothesNightVisionComponent/ClothesVisionSystem, which only exists for night vision.
/// Handled by <see cref="ClothesThermalVisionSystem"/>.
/// </summary>
[RegisterComponent]
public sealed partial class ClothesThermalVisionComponent : Component
{
    /// <summary>
    /// Whether this specific item added the wearer's ThermalVisionComponent (as opposed to the
    /// wearer already having thermal vision of their own, e.g. Homelander) - only then does
    /// unequipping remove it again.
    /// </summary>
    public bool Granted;
}
