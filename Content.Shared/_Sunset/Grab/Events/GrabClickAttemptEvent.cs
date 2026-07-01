namespace Content.Shared._Sunset.Grab.Events;

/// <summary>
/// Raised directed on the target of a Ctrl+Click ("TryPullObject") before the vanilla toggle-pull
/// logic runs. If a grab system marks it Handled, the vanilla pull toggle is skipped - used to turn
/// a re-click on an already-grabbed target into a grab escalation instead of releasing the pull.
/// </summary>
[ByRefEvent]
public struct GrabClickAttemptEvent(EntityUid user)
{
    public readonly EntityUid User = user;

    public bool Handled;
}
