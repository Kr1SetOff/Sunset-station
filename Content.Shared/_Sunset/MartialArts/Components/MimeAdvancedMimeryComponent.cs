namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Tracks the Invisible Blockade and Finger Guns actions granted while the Mime style is known, so
/// they can be removed again if the style is unlearned.
/// </summary>
[RegisterComponent]
public sealed partial class MimeAdvancedMimeryComponent : Component
{
    [DataField]
    public EntityUid? BlockadeAction;

    [DataField]
    public EntityUid? FingerGunsAction;
}
