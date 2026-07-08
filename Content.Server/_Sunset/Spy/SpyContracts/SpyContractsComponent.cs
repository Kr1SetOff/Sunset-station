using Content.Shared._Sunset.Spy.SpyContracts;
using Robust.Shared.Prototypes;

namespace Content.Server._Sunset.Spy.SpyContracts;

/// <summary>
/// Runtime state for one spy's contract board - see SpyContractSystem. Lives on the uplink item
/// itself (alongside its Store component) so a spy who drops/loses their uplink also loses access
/// to their board and reward payouts, same as they'd lose access to the shop.
/// </summary>
[RegisterComponent]
public sealed partial class SpyContractsComponent : Component
{
    /// <summary>Contracts currently offered on the board - refreshed every RotationInterval.</summary>
    [DataField]
    public List<ProtoId<SpyContractPrototype>> Board = new();

    /// <summary>Game time of the next board rotation.</summary>
    [DataField]
    public TimeSpan NextRotation;

    /// <summary>How often the board rerolls.</summary>
    [DataField]
    public TimeSpan RotationInterval = TimeSpan.FromMinutes(20);

    /// <summary>How many contracts are offered on the board at once.</summary>
    [DataField]
    public int BoardSize = 5;

    /// <summary>Currently accepted contract, if any.</summary>
    [DataField]
    public ProtoId<SpyContractPrototype>? ActiveContract;

    /// <summary>The concrete on-station target picked for the active contract.</summary>
    [DataField]
    public EntityUid? ActiveTarget;

    /// <summary>Reward locked in at accept time (computed from difficulty + reputation at that moment).</summary>
    [DataField]
    public int ActiveReward;

    /// <summary>How many contracts this spy has completed.</summary>
    [DataField]
    public int Completed;

    /// <summary>
    /// Reputation with the handler. Starts at 0, rises on completing a contract (by its
    /// difficulty weight), falls on abandoning an already-accepted one. Scales future rewards -
    /// see SpyContractSystem.GetReward.
    /// </summary>
    [DataField]
    public int Reputation;

    /// <summary>Is the active contract's SurveillanceProximity accumulation currently running?</summary>
    [DataField]
    public bool SurveillanceActive;

    /// <summary>Accumulated seconds spent within range of the SurveillanceProximity target.</summary>
    [DataField]
    public float Accumulated;

    /// <summary>Throttle for the once-a-second distance check.</summary>
    [DataField]
    public float CheckAccumulator;
}
