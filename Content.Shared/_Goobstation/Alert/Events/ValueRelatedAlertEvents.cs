using Content.Shared.Alert;

namespace Content.Shared._Goobstation.Alert.Events;

[ByRefEvent]
public record struct GetValueRelatedAlertValuesEvent(AlertPrototype Alert, float? MaxValue = null, float? CurrentValue = null, float MinValue = 0)
{
    public bool Handled => MaxValue.HasValue && CurrentValue.HasValue;
}
