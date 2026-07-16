using System.Collections.Generic;
using Robust.Shared.Serialization;

namespace Content.Shared.Research.Components
{
    [NetSerializable, Serializable]
    public enum ResearchConsoleUiKey : byte
    {
        Key,
    }

    [Serializable, NetSerializable]
    public sealed class ConsoleUnlockTechnologyMessage : BoundUserInterfaceMessage
    {
        public string Id;

        public ConsoleUnlockTechnologyMessage(string id)
        {
            Id = id;
        }
    }

    [Serializable, NetSerializable]
    public sealed class ConsoleServerSelectionMessage : BoundUserInterfaceMessage
    {

    }

    [Serializable, NetSerializable]
    public sealed class ResearchConsoleBoundInterfaceState : BoundUserInterfaceState
    {
        public int Points;

        /// <summary>
        /// 🌇Sunset🌇 - every technology's current availability, keyed by TechnologyPrototype ID.
        /// Drives node coloring on the branching tech tree console (see Content.Client._Sunset.Research).
        /// </summary>
        public Dictionary<string, ResearchAvailability> Researches;

        public ResearchConsoleBoundInterfaceState(int points, Dictionary<string, ResearchAvailability> researches)
        {
            Points = points;
            Researches = researches;
        }
    }

    /// <summary>
    /// 🌇Sunset🌇 - a technology's state relative to a specific research server, for tech-tree node
    /// coloring. Computed server-side in ResearchSystem.Console.cs using the same rules as
    /// SharedResearchSystem.IsTechnologyAvailable (discipline support, tier gating, prerequisites),
    /// split further by whether the server can currently afford it.
    /// </summary>
    [Serializable, NetSerializable]
    public enum ResearchAvailability : byte
    {
        /// <summary>Already unlocked.</summary>
        Researched,

        /// <summary>Prerequisites/discipline/tier are met and the server can afford it right now.</summary>
        Available,

        /// <summary>Prerequisites/discipline/tier are met, but the server can't afford it yet.</summary>
        PrereqsMet,

        /// <summary>Locked - prerequisites not unlocked, discipline unsupported, or tier not reached.</summary>
        Unavailable,
    }
}
