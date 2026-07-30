using Content.Shared.Inventory;

namespace Content.Shared._Goobstation.Flash;

// TODO: Sunset's vanilla Content.Shared/Flash/SharedFlashSystem.cs does not raise either of the
// events below (unlike Goob-Station's patched copy of that file). These shims let Changeling code
// referencing them compile, but CheckFlashVulnerable/FlashDurationMultiplierEvent will not actually
// affect flash behavior until someone wires RaiseLocalEvent calls for them into the real
// Content.Shared/Flash/SharedFlashSystem.cs (out of scope for this port - that file lives outside
// the _Goobstation folders this task is restricted to).

/// <summary>
/// Raised to check whether an entity should be forced to be vulnerable to flashes
/// regardless of any protection it may have (e.g. changeling augmented eyesight).
/// </summary>
[ByRefEvent]
public record struct CheckFlashVulnerable(
    bool Vulnerable);

/// <summary>
/// Raised to get a multiplier applied to the duration of an incoming flash.
/// </summary>
public sealed class FlashDurationMultiplierEvent : EntityEventArgs, IInventoryRelayEvent
{
    public float Multiplier = 1f;

    public SlotFlags TargetSlots => SlotFlags.EYES | SlotFlags.HEAD | SlotFlags.MASK;
}
