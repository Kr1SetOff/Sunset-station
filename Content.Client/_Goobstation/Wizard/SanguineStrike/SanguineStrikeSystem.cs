// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.SanguineStrike;

namespace Content.Client._Goobstation.Wizard.SanguineStrike;

// LifeStealOnProjectileHitSystem lives in Content.Shared and is auto-instantiated on both sides,
// so SharedSanguineStrikeSystem (which it injects) needs a concrete subclass on both sides too.
public sealed class SanguineStrikeSystem : SharedSanguineStrikeSystem;
