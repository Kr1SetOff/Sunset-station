using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.HotlineMiami;

/// <summary>
/// Deletes the remaining contents of a storage container as soon as the first item is taken out,
/// so a "pick 1 of N" crate can't just be emptied out entirely.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TakeOneItemStorageComponent : Component
{
    [DataField]
    public string ContainerId = "entity_storage";

    [DataField]
    public bool Triggered;
}
