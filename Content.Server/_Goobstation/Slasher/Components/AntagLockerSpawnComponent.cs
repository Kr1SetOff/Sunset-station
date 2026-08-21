using Robust.Shared.Map;

namespace Content.Server._Goobstation.Slasher.Components;

/// <summary>
/// When added to a game rule alongside AntagSelectionComponent,
/// places the ghost-role antag spawner inside a random station locker instead of an open tile.
/// Falls back to leaving the spawner where the rule put it if none are found.
/// </summary>
[RegisterComponent]
public sealed partial class AntagLockerSpawnComponent : Component
{
    /// <summary>
    /// When true, only lockers tagged MaintenanceCloset are eligible.
    /// </summary>
    [DataField]
    public bool MaintenanceOnly = true;

    [DataField]
    public EntityUid? ChosenLocker;

    /// <summary>
    /// Set when no eligible locker is found.
    /// </summary>
    [DataField]
    public EntityCoordinates? FallbackCoords;

    [DataField]
    public bool Placed;
}
