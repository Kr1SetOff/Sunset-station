namespace Content.Shared._Goobstation.Atmos.Events;

[ByRefEvent]
public record struct SendSafePressureEvent(
    float Pressure);

[ByRefEvent]
public record struct ResistPressureEvent
{
    public float Pressure;
    public bool Cancelled;
}
