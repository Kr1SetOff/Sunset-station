namespace Content.Server._Sunset.Photo;

[RegisterComponent]
public sealed partial class PhotoCardComponent : Component
{
    [DataField]
    public byte[]? ImageData;
}
