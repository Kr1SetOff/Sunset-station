namespace Content.Server._Goobstation.Religion.Nullrod;

/// <summary>
/// Ported from Goob-Station's RecallPrayableComponent (trimmed down to plain recall - Goob's
/// Unremoveable/DualWield/Embedded nullrod variants aren't ported). Placed on chapel altars so a
/// chaplain who bound a nullrod to themselves (NullrodSystem's bind verb) can pray it back into
/// their hand from anywhere on the station.
/// </summary>
[RegisterComponent]
public sealed partial class RecallNullrodComponent : Component
{
    /// <summary>
    /// How long the recall do-after takes to complete.
    /// </summary>
    [DataField]
    public TimeSpan DoAfterDuration = TimeSpan.FromSeconds(5);
}
