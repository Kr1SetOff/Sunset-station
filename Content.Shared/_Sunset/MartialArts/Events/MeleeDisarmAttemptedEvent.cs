namespace Content.Shared._Sunset.MartialArts.Events;

/// <summary>
/// Raised directed on the user right after a disarm/shove attempt passes basic validation (range,
/// target state) but before the random success chance is rolled. Used to record a "Disarm" combo
/// input on any attempt - including plain shoves with nothing to disarm - matching how Goob Station
/// counts these towards martial arts combos.
/// </summary>
[ByRefEvent]
public readonly record struct MeleeDisarmAttemptedEvent(EntityUid User, EntityUid Target);
