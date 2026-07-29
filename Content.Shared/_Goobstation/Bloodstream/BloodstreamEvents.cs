// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Goobstation.Bloodstream;

/// <summary>
/// Raised on an entity when it stops taking bloodloss damage (e.g. bleeding fully stopped).
/// </summary>
public sealed class StoppedTakingBloodlossDamageEvent : EntityEventArgs;

/// <summary>
/// Raised on an entity to collect multipliers applied to bloodloss damage.
/// Ported alongside the Wizard "Scream For Me" spell (<see cref="Content.Server._Goobstation.Wizard.Components.BloodlossDamageMultiplierComponent"/>).
/// Note: Sunset's vanilla bloodstream/bloodloss damage code does not currently raise this event, so the
/// multiplier only takes effect if something raises it - the component/event pair is ported as-is (matching
/// Goob-Station) for compatibility, but is otherwise inert until a bloodloss-damage system is wired up to it.
/// </summary>
public sealed class GetBloodlossDamageMultiplierEvent(float multiplier = 1f) : EntityEventArgs
{
    public float Multiplier = multiplier;
}
