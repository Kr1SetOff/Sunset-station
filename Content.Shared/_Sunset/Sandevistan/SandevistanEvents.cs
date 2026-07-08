using Content.Shared.Actions;

namespace Content.Shared._Sunset.Sandevistan;

public sealed partial class ToggleSandevistanEvent : InstantActionEvent;

/// <summary>
/// Raised to remove slowdown from an entity affected by a sandevistan slowfield.
/// </summary>
[ByRefEvent]
public record struct RemoveSandevistanSlowdownEvent(EntityUid Source);
