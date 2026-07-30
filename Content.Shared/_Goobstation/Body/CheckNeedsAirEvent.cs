namespace Content.Shared._Goobstation.Body;

[ByRefEvent]
public record struct CheckNeedsAirEvent(
    bool Cancelled);
