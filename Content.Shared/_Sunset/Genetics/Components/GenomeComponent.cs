// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Genetics.Components;

/// <summary>
///     Holds the genetic code of an entity, mirroring the SS13 genome.
///     The code is split into three categories of hexadecimal blocks:
///     <list type="bullet">
///         <item>UI (Unique Identifiers) — cosmetic data (skin/eye colour, sex).</item>
///         <item>UE (Unique Enzymes) — identity, mainly the displayed name.</item>
///         <item>SE (Structural Enzymes) — structure, holds powers and diseases.</item>
///     </list>
///     Every block is a value in the range 0x000..0xFFF, i.e. three hexadecimal "subblocks".
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class GenomeComponent : Component
{
    public const int UiBlockCount = 36;
    public const int UeBlockCount = 14;
    public const int SeBlockCount = 56; // indices 0..55, SS13 uses 1..55

    public const int MaxBlock = 0xFFF;

    // UI block layout. Colours are stored as RGB triplets across three blocks each.
    public const int BlockSkinR = 13;
    public const int BlockSkinG = 14;
    public const int BlockSkinB = 15;
    public const int BlockEyeR = 17;
    public const int BlockEyeG = 18;
    public const int BlockEyeB = 19;

    /// <summary>Sex block. Values below <see cref="SexThreshold"/> are female, otherwise male.</summary>
    public const int BlockSex = 32;
    public const int SexThreshold = 0x600;

    /// <summary>SE block that determines whether the carrier is a humanoid (below 0x320) or an animal.</summary>
    public const int BlockSpecies = 55;

    [DataField, AutoNetworkedField]
    public List<int> Ui = new();

    [DataField, AutoNetworkedField]
    public List<int> Ue = new();

    [DataField, AutoNetworkedField]
    public List<int> Se = new();

    /// <summary>Current genetic instability. High values cause burns, deformity and eventually death.</summary>
    [DataField, AutoNetworkedField]
    public float Instability;

    /// <summary>IDs of the mutations currently expressed by this genome.</summary>
    [DataField, AutoNetworkedField]
    public List<string> ActiveMutations = new();

    /// <summary>Whether the genome has been randomized/initialized yet.</summary>
    [DataField, AutoNetworkedField]
    public bool Generated;

    /// <summary>Hash of the UE block list that was last applied to the carrier's name.</summary>
    [DataField]
    public int AppliedUeHash;

    /// <summary>
    ///     Server-side bookkeeping: action entities granted per active mutation so they can be revoked.
    ///     Not networked — action grants are handled entirely on the server.
    /// </summary>
    [ViewVariables]
    public Dictionary<string, List<EntityUid>> GrantedActions = new();
}
