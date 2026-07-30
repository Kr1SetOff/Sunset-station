// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.BindSoul;

namespace Content.Server._Goobstation.Wizard.BindSoul;

// SharedBindSoulSystem was ported without a concrete subclass anywhere, which left it unregistered
// for dependency injection (SpellsSystem injects it directly) and crashed the YAML linter on startup.
// This registers it with the base class's default (no-op) Resurrect/RespawnItem/MakeDestructible
// behavior - Goob's actual resurrection trigger (how a ghosted lich gets back into a new body via
// their phylactery) was never ported, so BindSoul currently only handles the "die and get ghosted
// near your phylactery" half of the mechanic, not the "come back" half.
public sealed class BindSoulSystem : SharedBindSoulSystem;
