// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Granted by the coughing disease gene. A marker (rather than a raw AutoEmoteComponent grant) so this
///     can be active alongside GeneSneezing/GeneHiccuping without one gene's activation overwriting another's
///     AutoEmoteComponent. Wired to the shared auto-emote system by
///     <see cref="Content.Server._Sunset.Genetics.GeneVocalTicSystem"/>.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class GeneCoughingComponent : Component;
