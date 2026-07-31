// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Collections.Generic;
using Robust.Shared.GameObjects;
using Content.Shared._Goobstation.Wizard.SanguineStrike;

namespace Content.Server._Goobstation.Wizard.SanguineStrike;

// SharedSanguineStrikeSystem was ported without a concrete subclass anywhere, which left it
// unregistered for dependency injection (LifeStealOnProjectileHitSystem injects it directly) and
// crashed the YAML linter on startup.
public sealed class SanguineStrikeSystem : SharedSanguineStrikeSystem
{
    // Fixed: Hit() was never overridden, so the component granted by casting Exsanguinating Strike
    // was never removed after landing a blow - "your NEXT melee attack" (per its own description
    // and the spell-fail-sanguine-strike-already-empowered check that assumes single use) was
    // instead a permanent enchantment for as long as you held the weapon.
    protected override void Hit(EntityUid uid,
        SanguineStrikeComponent component,
        EntityUid user,
        IReadOnlyList<EntityUid> hitEntities)
    {
        base.Hit(uid, component, user, hitEntities);

        RemComp<SanguineStrikeComponent>(uid);
    }
}
