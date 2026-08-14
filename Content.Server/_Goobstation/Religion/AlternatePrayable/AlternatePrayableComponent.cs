namespace Content.Server._Goobstation.Religion.AlternatePrayable;

/// <summary>
/// Ported from Goob-Station's Religion system. Not to be confused with Content.Shared.Prayer's
/// PrayableComponent (the classic "send a subtle message to admins" verb) - this is a distinct
/// mechanic where using an item on the entity that has this component raises an
/// <see cref="AlternatePrayEvent"/> on it after a do-after, letting other components (e.g.
/// HealNearOnPray) react to being "prayed at".
/// </summary>
[RegisterComponent]
public sealed partial class AlternatePrayableComponent : Component
{
    /// <summary>
    /// How long the praying do-after takes to complete.
    /// </summary>
    [DataField]
    public TimeSpan PrayDoAfterDuration = TimeSpan.FromSeconds(5);

    [ViewVariables]
    public TimeSpan PopupDelay = TimeSpan.FromSeconds(3);

    [ViewVariables]
    public TimeSpan NextPopup;

    /// <summary>
    /// Should the prayer repeat endlessly until cancelled?
    /// </summary>
    [DataField]
    public bool RepeatPrayer;

    /// <summary>
    /// Does the user have to be a bible user to pray at this?
    /// </summary>
    [DataField]
    public bool RequireBibleUser = true;
}
