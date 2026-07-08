using Content.Shared.Actions;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Flight;

public sealed partial class ToggleFlightEvent : InstantActionEvent;

public sealed class FlightEvent : EntityEventArgs
{
    public EntityUid Uid { get; }
    public bool IsFlying { get; }

    public FlightEvent(EntityUid uid, bool isFlying)
    {
        Uid = uid;
        IsFlying = isFlying;
    }
}

[ByRefEvent]
public sealed class FlightAttemptEvent : CancellableEntityEventArgs;

[Serializable, NetSerializable]
public sealed class ToggleFlightVisualsEvent : EntityEventArgs
{
    public NetEntity Uid { get; }
    public bool IsFlying { get; }

    public ToggleFlightVisualsEvent(NetEntity uid, bool isFlying)
    {
        Uid = uid;
        IsFlying = isFlying;
    }
}
