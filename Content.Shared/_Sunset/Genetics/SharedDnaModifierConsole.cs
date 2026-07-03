// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Genetics;

/// <summary>The three categories of genetic blocks editable from the DNA modifier console.</summary>
[Serializable, NetSerializable]
public enum GenomeCategory : byte
{
    Ui,
    Ue,
    Se,
}

[Serializable, NetSerializable]
public enum DnaModifierConsoleUiKey : byte
{
    Key,
}

/// <summary>Appearance key for the DNA modifier machine sprite.</summary>
[Serializable, NetSerializable]
public enum DnaModifierVisuals : byte
{
    Status,
}

/// <summary>Sprite states the DNA modifier machine can show, based on its occupant.</summary>
[Serializable, NetSerializable]
public enum DnaModifierStatus : byte
{
    /// <summary>No occupant inserted.</summary>
    Idle,

    /// <summary>A living subject is inside.</summary>
    Occupied,

    /// <summary>A dead subject is inside.</summary>
    Gore,
}

/// <summary>Transfer-buffer operations available from the console.</summary>
[Serializable, NetSerializable]
public enum DnaBufferAction : byte
{
    SaveSe,
    SaveUi,
    SaveUiUe,
    Apply,
    Injector,
    Clear,
}

/// <summary>A mutation this console has researched, with its display strings pre-resolved server-side.</summary>
[Serializable, NetSerializable]
public sealed class DiscoveredMutationInfo
{
    public readonly string Id;
    public readonly string Name;
    public readonly string Description;

    /// <summary>True when the mutation is currently expressed by the machine's occupant.</summary>
    public readonly bool ActiveOnOccupant;

    public DiscoveredMutationInfo(string id, string name, string description, bool activeOnOccupant)
    {
        Id = id;
        Name = name;
        Description = description;
        ActiveOnOccupant = activeOnOccupant;
    }
}

/// <summary>State pushed to the DNA modifier console UI.</summary>
[Serializable, NetSerializable]
public sealed class DnaModifierConsoleBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly bool MachineConnected;
    public readonly bool MachineInRange;
    public readonly bool HasOccupant;
    public readonly string OccupantName;
    public readonly List<int> Ui;
    public readonly List<int> Ue;
    public readonly List<int> Se;
    public readonly float Instability;
    public readonly List<string> ActiveMutations;
    public readonly List<bool> BufferFilled;
    public readonly List<DiscoveredMutationInfo> DiscoveredMutations;

    public DnaModifierConsoleBoundUserInterfaceState(
        bool machineConnected,
        bool machineInRange,
        bool hasOccupant,
        string occupantName,
        List<int> ui,
        List<int> ue,
        List<int> se,
        float instability,
        List<string> activeMutations,
        List<bool> bufferFilled,
        List<DiscoveredMutationInfo> discoveredMutations)
    {
        MachineConnected = machineConnected;
        MachineInRange = machineInRange;
        HasOccupant = hasOccupant;
        OccupantName = occupantName;
        Ui = ui;
        Ue = ue;
        Se = se;
        Instability = instability;
        ActiveMutations = activeMutations;
        BufferFilled = bufferFilled;
        DiscoveredMutations = discoveredMutations;
    }
}

/// <summary>Precisely irradiate one subblock of one block by <see cref="Delta"/> (+1 or -1).</summary>
[Serializable, NetSerializable]
public sealed class DnaModifierRadiateMessage : BoundUserInterfaceMessage
{
    public readonly GenomeCategory Category;
    public readonly int Block;
    public readonly int SubBlock;
    public readonly int Delta;

    public DnaModifierRadiateMessage(GenomeCategory category, int block, int subBlock, int delta)
    {
        Category = category;
        Block = block;
        SubBlock = subBlock;
        Delta = delta;
    }
}

/// <summary>Randomly mutate a block within a category (the SS13 "Pulse Radiation").</summary>
[Serializable, NetSerializable]
public sealed class DnaModifierPulseMessage : BoundUserInterfaceMessage
{
    public readonly GenomeCategory Category;

    public DnaModifierPulseMessage(GenomeCategory category)
    {
        Category = category;
    }
}

[Serializable, NetSerializable]
public sealed class DnaModifierEjectMessage : BoundUserInterfaceMessage
{
}

/// <summary>Request to print a single-use gene activator injector for a discovered mutation.</summary>
[Serializable, NetSerializable]
public sealed class DnaModifierPrintActivatorMessage : BoundUserInterfaceMessage
{
    public readonly string MutationId;

    public DnaModifierPrintActivatorMessage(string mutationId)
    {
        MutationId = mutationId;
    }
}

/// <summary>Operate the transfer buffer slot <see cref="Slot"/> with <see cref="Action"/>.</summary>
[Serializable, NetSerializable]
public sealed class DnaModifierBufferMessage : BoundUserInterfaceMessage
{
    public readonly DnaBufferAction Action;
    public readonly int Slot;

    public DnaModifierBufferMessage(DnaBufferAction action, int slot)
    {
        Action = action;
        Slot = slot;
    }
}
