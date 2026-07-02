namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Tracks the Invisible Blockade action granted while the Mime style is known, so it can be removed
/// again if the style is unlearned.
/// </summary>
[RegisterComponent]
public sealed partial class MimeAdvancedMimeryComponent : Component
{
    [DataField]
    public EntityUid? BlockadeAction;
}
