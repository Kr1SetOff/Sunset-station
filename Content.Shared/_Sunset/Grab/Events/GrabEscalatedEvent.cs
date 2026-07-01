using Content.Shared._Sunset.Grab.Components;

namespace Content.Shared._Sunset.Grab.Events;

/// <summary>
/// Raised directed on the grabber whenever their grab stage successfully advances (Passive -&gt;
/// Aggressive -&gt; Choke). Used by other systems (e.g. martial arts combos) that care about "a grab
/// escalation just happened" without needing to hook the escalation logic itself.
/// </summary>
[ByRefEvent]
public readonly record struct GrabEscalatedEvent(EntityUid Grabbed, GrabStage NewStage);
