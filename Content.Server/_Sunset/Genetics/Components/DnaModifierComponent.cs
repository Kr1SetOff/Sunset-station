// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Containers;

namespace Content.Server._Sunset.Genetics.Components;

/// <summary>
///     A DNA modifier: a machine that holds an occupant whose genome can be irradiated from a
///     <see cref="DnaModifierConsoleComponent"/>. Mirrors the medical scanner occupant pattern.
/// </summary>
[RegisterComponent]
public sealed partial class DnaModifierComponent : Component
{
    public const string MachinePort = "DnaModifierReceiver";

    public ContainerSlot BodyContainer = default!;

    [ViewVariables]
    public EntityUid? ConnectedConsole;

    /// <summary>
    ///     True while an occupant is being ejected. Prevents the ClimbedOnEvent raised by
    ///     <see cref="Content.Shared.Climbing.Systems.ClimbSystem.ForciblySetClimbing"/> during ejection
    ///     from immediately re-inserting the body.
    /// </summary>
    [ViewVariables]
    public bool Ejecting;

    /// <summary>Chance per irradiation to also flip an extra random subblock.</summary>
    [DataField]
    public float ExtraMutationChance = 0.25f;

    /// <summary>Toxin damage dealt to the occupant per irradiation.</summary>
    [DataField]
    public float ToxinPerPulse = 2f;

    /// <summary>Radiation damage dealt to the occupant per irradiation.</summary>
    [DataField]
    public float RadiationPerPulse = 4f;
}
