using Content.Shared._Sunset.Spy.SpyContracts;
using Robust.Shared.Prototypes;

namespace Content.Server._Sunset.Spy.SpyContracts;

/// <summary>
/// Added to a Spy Tracker item (SpyTrackerComponent) once it's planted on a contract's target (see
/// SpyContractSystem.PlantTracker) - it's still the very same item, not a separate entity, so it
/// keeps its normal name/description/sprite throughout. The bug sits wherever it was dropped, counts
/// down EndTime on its own regardless of whether the spy stays nearby, and is a completely ordinary
/// pickup-able item to anyone who walks up and grabs it - if that happens before EndTime, the
/// surveillance is blown (SpyContractSystem.OnPlantedTrackerPickedUp fails the spy's contract);
/// either way, picking it up removes this component again, turning it back into a normal,
/// re-plantable Spy Tracker.
/// </summary>
[RegisterComponent]
public sealed partial class SpyPlantedTrackerComponent : Component
{
    /// <summary>The spy who planted this, for popups and to know who to pay out / fail.</summary>
    [DataField]
    public EntityUid Spy;

    /// <summary>The spy's SpyContractsComponent-bearing uplink item.</summary>
    [DataField]
    public EntityUid Uplink;

    /// <summary>The contract this bug is working - must still match the uplink's ActiveContract for
    /// this bug to actually count toward anything (it won't, e.g. after the contract was abandoned).</summary>
    [DataField]
    public ProtoId<SpyContractPrototype> ContractId;

    /// <summary>The concrete target this contract was working, passed through to CompleteContract
    /// (e.g. for ApplySabotage).</summary>
    [DataField]
    public EntityUid? Target;

    /// <summary>When this bug finishes its job and the contract completes, absent early discovery.</summary>
    [DataField]
    public TimeSpan EndTime;

    /// <summary>Throttle for the once-a-second progress/UI update.</summary>
    [DataField]
    public float CheckAccumulator;
}
