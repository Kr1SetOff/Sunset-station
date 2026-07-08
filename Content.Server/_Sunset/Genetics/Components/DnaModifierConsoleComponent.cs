// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Genetics;

namespace Content.Server._Sunset.Genetics.Components;

/// <summary>
///     Console that drives a linked <see cref="DnaModifierComponent"/>. Mirrors the cloning console.
/// </summary>
[RegisterComponent]
public sealed partial class DnaModifierConsoleComponent : Component
{
    public const string MachinePort = "DnaModifierSender";

    public const int BufferCount = 3;

    [ViewVariables]
    public EntityUid? Machine;

    /// <summary>Maximum distance between the console and its machine.</summary>
    [DataField]
    public float MaxDistance = 4f;

    public bool MachineInRange = true;

    /// <summary>Transfer buffer slots holding saved genome subsets.</summary>
    [ViewVariables]
    public GenomeSnapshot?[] Buffers = new GenomeSnapshot?[BufferCount];

    /// <summary>
    ///     Mutations the current occupant is expressing right now, by prototype ID. This is recomputed from the
    ///     occupant on every UI refresh and is NOT persisted: when a mutation leaves the subject (or the machine
    ///     empties) it clears from here too. Only these mutations can be printed as activators.
    /// </summary>
    [ViewVariables]
    public HashSet<string> DiscoveredMutations = new();

    /// <summary>Injector entity spawned by the buffer (dropped at the console).</summary>
    [DataField]
    public string InjectorPrototype = "GeneticInjector";

    /// <summary>Gene activator injector entity spawned for a discovered mutation (dropped at the console).</summary>
    [DataField]
    public string ActivatorInjectorPrototype = "GeneticInjectorActivator";

    /// <summary>Cooldown between printing activator injectors.</summary>
    [DataField]
    public TimeSpan PrintCooldown = TimeSpan.FromMinutes(2.5);

    /// <summary>Earliest time the console may print another activator injector.</summary>
    [DataField]
    public TimeSpan NextPrint = TimeSpan.Zero;
}
