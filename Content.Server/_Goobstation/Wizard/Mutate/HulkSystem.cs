// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Wizard.Mutate;

namespace Content.Server._Goobstation.Wizard.Mutate;

// SharedHulkSystem was ported without a concrete subclass anywhere, which left it unregistered
// for dependency injection and unable to run its Initialize()/event subscriptions at all.
public sealed class HulkSystem : SharedHulkSystem;
