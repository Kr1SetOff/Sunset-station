namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Placed on a book/scroll item entity. Using it in-hand teaches the user the given martial art
/// style (if they don't already know one) and consumes the item.
/// </summary>
[RegisterComponent]
public sealed partial class MartialArtManualComponent : Component
{
    [DataField(required: true)]
    public MartialArtStyle Style;
}
