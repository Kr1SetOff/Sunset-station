// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Genetics;

/// <summary>
///     The activation threshold of a structural-enzyme (SE) mutation, mirroring the SS13 tiers.
///     A block must reach this hexadecimal value for the gene to express itself.
/// </summary>
public enum MutationTier
{
    /// <summary>Minor powers and all diseases activate at 0x802 and above.</summary>
    Minor = 0x802,

    /// <summary>Intermediate powers activate at 0xBEA and above.</summary>
    Intermediate = 0xBEA,

    /// <summary>Major powers activate at 0xDAC and above.</summary>
    Major = 0xDAC,
}

/// <summary>
///     Defines a single genetic mutation (power or disease) that lives in a structural-enzyme block.
///     Which block it occupies is shuffled every round (Paradise SS13 style - see
///     GeneticsSystem.EnsureGeneLayout), so geneticists rediscover positions each shift.
///     When the block value reaches the activation threshold the mutation expresses itself by adding
///     <see cref="Components"/> and granting <see cref="Actions"/> to the carrier.
/// </summary>
[Prototype]
public sealed partial class MutationPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    /// <summary>Localized display name shown on the DNA console.</summary>
    [DataField]
    public LocId Name = string.Empty;

    /// <summary>Localized description shown on the DNA console.</summary>
    [DataField]
    public LocId Description = string.Empty;

    /// <summary>Activation threshold for this gene. Diseases ignore this and always use <see cref="MutationTier.Minor"/>.</summary>
    [DataField]
    public MutationTier Tier = MutationTier.Minor;

    /// <summary>If true this is a disease: it activates at 0x802 and removes instability instead of adding it.</summary>
    [DataField]
    public bool Disease;

    /// <summary>
    ///     Genetic instability contributed while this mutation is active.
    ///     Powers should use positive values, diseases negative ones.
    /// </summary>
    [DataField]
    public float Instability = 5f;

    /// <summary>Components added to the carrier while the mutation is active, removed when it deactivates.</summary>
    [DataField(serverOnly: true)]
    public ComponentRegistry Components = new();

    /// <summary>Action prototypes granted to the carrier while active, revoked when the mutation deactivates.</summary>
    [DataField]
    public List<EntProtoId> Actions = new();
}
