using Content.Shared._Goobstation.InternalResources.Data;

namespace Content.Shared._Goobstation.InternalResources.Events;

[ByRefEvent]
public record struct GetInternalResourcesCostModifierEvent(EntityUid Target, InternalResourcesData Data, float Multiplier = 1);
