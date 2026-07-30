// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.BindSoul;

namespace Content.Client._Goobstation.Wizard.BindSoul;

// SharedSpellsSystem injects this, and now has a client-side concrete subclass too, so this needs
// one as well.
public sealed class BindSoulSystem : SharedBindSoulSystem;
