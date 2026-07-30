// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.TeslaBlast;

namespace Content.Client._Goobstation.Wizard.TeslaBlast;

// SharedSpellsSystem injects this, and now has a client-side concrete subclass too, so this needs
// one as well.
public sealed class TeslaBlastSystem : SharedTeslaBlastSystem;
