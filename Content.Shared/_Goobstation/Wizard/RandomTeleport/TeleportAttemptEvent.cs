// SPDX-License-Identifier: AGPL-3.0-01-later

namespace Content.Shared._Goobstation.Wizard.RandomTeleport;

[ByRefEvent]
public record struct TeleportAttemptEvent(bool Cancelled);
