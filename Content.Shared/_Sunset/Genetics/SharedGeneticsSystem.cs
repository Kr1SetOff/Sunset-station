// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Genetics.Components;

namespace Content.Shared._Sunset.Genetics;

/// <summary>
///     Shared helpers for reading and writing genetic blocks and their hexadecimal subblocks.
///     A block is a value 0x000..0xFFF made of three subblocks (high/mid/low nibbles).
/// </summary>
public abstract class SharedGeneticsSystem : EntitySystem
{
    /// <summary>
    ///     Reads one subblock from a block.
    ///     <paramref name="pos"/> 0 is the high (hundreds) nibble, 1 the mid, 2 the low.
    /// </summary>
    public static int GetSubBlock(int block, int pos)
    {
        var shift = (2 - pos) * 4;
        return (block >> shift) & 0xF;
    }

    /// <summary>Returns a copy of <paramref name="block"/> with the subblock at <paramref name="pos"/> set to <paramref name="value"/>.</summary>
    public static int SetSubBlock(int block, int pos, int value)
    {
        var shift = (2 - pos) * 4;
        var mask = 0xF << shift;
        return (block & ~mask) | ((value & 0xF) << shift);
    }

    public static int ClampBlock(int block)
    {
        return Math.Clamp(block, 0, GenomeComponent.MaxBlock);
    }

    /// <summary>True when a structural-enzyme block has reached the activation threshold for a mutation.</summary>
    public static bool IsMutationActive(int blockValue, MutationTier tier, bool disease)
    {
        var threshold = disease ? (int) MutationTier.Minor : (int) tier;
        return blockValue >= threshold;
    }

    /// <summary>Converts a 0x000..0xFFF block into a 0..255 colour channel.</summary>
    public static byte BlockToChannel(int block)
    {
        return (byte) (Math.Clamp(block, 0, GenomeComponent.MaxBlock) * 255 / GenomeComponent.MaxBlock);
    }

    /// <summary>Converts a 0..255 colour channel into a 0x000..0xFFF block.</summary>
    public static int ChannelToBlock(byte value)
    {
        return value * GenomeComponent.MaxBlock / 255;
    }

    public static Color BlocksToColor(int r, int g, int b)
    {
        return new Color(BlockToChannel(r), BlockToChannel(g), BlockToChannel(b));
    }

    /// <summary>Stable hash of a block list, used to detect when the UE (identity) has changed.</summary>
    public static int HashBlocks(IReadOnlyList<int> blocks)
    {
        var hash = 17;
        foreach (var value in blocks)
            hash = hash * 31 + value;
        return hash;
    }
}
