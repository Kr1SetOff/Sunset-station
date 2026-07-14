using Content.Server._Sunset.MartialArts.Systems;

namespace Content.Server._Sunset.MartialArts.Components;

/// <summary>
/// Entities with this component periodically scan their surroundings and treat anyone who has
/// learned the Sleeping Carp martial art as hostile (via NpcFactionSystem's per-entity aggro
/// exception), regardless of faction. Used by Shiva, who doesn't tolerate a rival martial artist
/// on her station.
/// </summary>
[RegisterComponent, Access(typeof(HatesSleepingCarpSystem))]
public sealed partial class HatesSleepingCarpComponent : Component
{
    /// <summary>
    /// How often to rescan for nearby Sleeping Carp practitioners.
    /// </summary>
    [DataField]
    public TimeSpan ScanInterval = TimeSpan.FromSeconds(2);

    /// <summary>
    /// How far to scan for practitioners.
    /// </summary>
    [DataField]
    public float ScanRange = 10f;

    public TimeSpan NextScan = TimeSpan.Zero;
}
