using Content.Shared.EntityTable.EntitySelectors;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Photography;

/// <summary>
/// Marks an entity as able to take pictures (when you flash other entities with it).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class PictureTakerComponent : Component
{
    /// <summary>
    /// The entities that will be spawned & given a PhotographComponent when the owning entity is used.
    /// The table should only select one item at a time.
    /// </summary>
    [DataField]
    public EntityTableSelector? Photographs;
}
