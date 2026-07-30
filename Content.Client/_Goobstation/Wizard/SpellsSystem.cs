// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard;

namespace Content.Client._Goobstation.Wizard;

// SharedSpellsSystem only had a concrete subclass on the server (Content.Server._Goobstation.Wizard.
// Systems.SpellsSystem), but it's injected by shared systems like SwapOnProjectileHitSystem that get
// auto-instantiated on both sides, so it needs a client-side registration too. CreateChargeEffect is
// the one genuinely abstract member - the server's override broadcasts a network event for clients to
// react to, so the client itself has nothing to do here.
public sealed class SpellsSystem : SharedSpellsSystem
{
    protected override void CreateChargeEffect(EntityUid uid, ChargeSpellRaysEffectEvent ev)
    {
    }
}
