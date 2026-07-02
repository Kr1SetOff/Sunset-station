namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Unlike <see cref="MartialArtManualComponent"/> (a single-use consumable), this grants its style for
/// as long as the item stays equipped (e.g. a judo belt) and revokes it again the moment it's removed.
/// </summary>
[RegisterComponent]
public sealed partial class GrantMartialArtOnEquipComponent : Component
{
    [DataField(required: true)]
    public MartialArtStyle Style;
}
