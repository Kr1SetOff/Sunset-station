// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.TeslaBlast;

namespace Content.Server._Goobstation.Wizard.TeslaBlast;

// SharedTeslaBlastSystem was ported without a concrete subclass anywhere, which left it unregistered
// for dependency injection (SharedSpellsSystem injects it directly) and crashed the YAML linter on
// startup.
public sealed class TeslaBlastSystem : SharedTeslaBlastSystem;
