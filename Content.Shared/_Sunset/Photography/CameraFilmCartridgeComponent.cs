namespace Content.Shared._Sunset.Photography;

/// <summary>
/// A roll of camera film. Insert into a camera's film slot to reload it; the cartridge is consumed.
/// </summary>
[RegisterComponent]
public sealed partial class CameraFilmCartridgeComponent : Component
{
    /// <summary>
    /// How many shots this cartridge adds to the camera it's inserted into.
    /// </summary>
    [DataField]
    public int Charges = 6;
}
