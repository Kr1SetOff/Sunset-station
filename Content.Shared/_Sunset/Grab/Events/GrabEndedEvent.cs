namespace Content.Shared._Sunset.Grab.Events;

/// <summary>
/// Raised directed on the grabber whenever their grab ends, for any reason (target escaped/was
/// thrown, puller let go, either party was deleted). Used by other systems (e.g. martial arts
/// combos) that care about "a grab just ended" without needing to hook GrabberComponent's own
/// lifecycle directly - SharedGrabSystem already owns the one allowed ComponentShutdown subscriber
/// slot for GrabberComponent, so a second one collides at startup (Robust only allows one directed
/// subscriber per component+event pair, system-wide).
/// </summary>
[ByRefEvent]
public readonly record struct GrabEndedEvent(EntityUid Grabbed);
