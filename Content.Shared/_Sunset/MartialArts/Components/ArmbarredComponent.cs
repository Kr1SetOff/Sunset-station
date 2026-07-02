namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Marks a downed entity as being held in a Corporate Judo armbar. Only the entity that applied the
/// armbar (<see cref="Puller"/>) can follow up with Wheel Throw. Cleared automatically if the hold is
/// broken (the victim stands up or the pull holding them stops).
/// </summary>
[RegisterComponent]
public sealed partial class ArmbarredComponent : Component
{
    [DataField]
    public EntityUid Puller;
}
