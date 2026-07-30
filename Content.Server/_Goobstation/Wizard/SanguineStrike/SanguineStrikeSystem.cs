// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.SanguineStrike;

namespace Content.Server._Goobstation.Wizard.SanguineStrike;

// SharedSanguineStrikeSystem was ported without a concrete subclass anywhere, which left it
// unregistered for dependency injection (LifeStealOnProjectileHitSystem injects it directly) and
// crashed the YAML linter on startup.
public sealed class SanguineStrikeSystem : SharedSanguineStrikeSystem;
