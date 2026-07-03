// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Genetics;

/// <summary>
///     A saved subset of a genome (the SS13 "transfer buffer" payload). Each flag marks which
///     categories of blocks were captured and should be re-applied.
/// </summary>
[Serializable, NetSerializable, DataDefinition]
public sealed partial class GenomeSnapshot
{
    [DataField]
    public List<int> Ui = new();

    [DataField]
    public List<int> Ue = new();

    [DataField]
    public List<int> Se = new();

    [DataField]
    public bool HasUi;

    [DataField]
    public bool HasUe;

    [DataField]
    public bool HasSe;
}
