namespace Content.Shared._Sunset.Teleporter;

// 🌇Sunset🌇 - ported from Goobstation/Reserve-Station's BlockTeleport event, trimmed to just the
// struct - nothing in this fork currently blocks teleportation, so raising this is a harmless no-op
// hook for now, kept in case something wants to cancel a teleport later.
[ByRefEvent]
public record struct TeleportAttemptEvent(
    bool Predicted = true,
    string? Message = "teleport-blocked-message",
    bool Cancelled = false);
