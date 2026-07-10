namespace Content.Shared._Sunset.Photography;

/// <summary>
/// Marks a camera as reloadable with a <see cref="CameraFilmCartridgeComponent"/>. The actual slot is
/// defined by an ItemSlots component on the same entity, matching the key given here (same pattern as
/// PowerCellSlotComponent's CellSlotId).
/// </summary>
[RegisterComponent]
public sealed partial class CameraFilmSlotComponent : Component
{
    [DataField(required: true)]
    public string SlotId = string.Empty;
}
