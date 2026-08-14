namespace Content.Server._Goobstation.Religion.AlternatePrayable;

/// <summary>
/// Raised on an entity with AlternatePrayableComponent once the pray do-after completes, letting
/// other components (HealNearOnPraySystem, etc) react to being "prayed at". Purely server-side and
/// local (never networked), unlike AlternatePrayDoAfterEvent (Content.Shared._Goobstation.Religion).
/// </summary>
[ByRefEvent]
public record struct AlternatePrayEvent(EntityUid User);
