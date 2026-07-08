using Content.Shared.Roles;
using Content.Shared.Whitelist;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Spy.SpyContracts;

/// <summary>
/// A Spy contract template (Bounty System, ported from frostek3122-byte/sunset-station). Contracts
/// are the Spy's unique objective system: instead of round-start objectives, the spy accepts them
/// directly from the contract board in their uplink. The prototype only describes the contract;
/// picking the concrete on-station target, tracking progress, and paying out is all
/// Content.Server._Sunset.Spy.SpyContracts.SpyContractSystem.
/// </summary>
[Prototype]
public sealed partial class SpyContractPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField("contractType")]
    public SpyContractType Type = SpyContractType.Surveillance;

    /// <summary>Difficulty gates default reward/reputation weight and which board slot it can roll into.</summary>
    [DataField]
    public SpyContractDifficulty Difficulty = SpyContractDifficulty.Medium;

    [DataField]
    public LocId Name = string.Empty;

    [DataField]
    public LocId Description = string.Empty;

    /// <summary>SpyCredit reward. 0 (default) means "compute from Difficulty" - see SpyContractSystem.</summary>
    [DataField]
    public int Reward;

    /// <summary>Seconds the Spy Tracker needs to work on the target (surveillance hold / sabotage / data pull).</summary>
    [DataField]
    public float Duration = 45f;

    /// <summary>Tiles the spy must stay within the target for SpyContractType.SurveillanceProximity.</summary>
    [DataField]
    public float Radius = 4f;

    /// <summary>What entities on the station can be this contract's target.</summary>
    [DataField]
    public EntityWhitelist? TargetWhitelist;

    /// <summary>Optional job filter (e.g. watch/kill specifically the Captain).</summary>
    [DataField]
    public List<ProtoId<JobPrototype>> TargetJobs = new();

    /// <summary>Effect applied to the target on completion of a Sabotage contract.</summary>
    [DataField]
    public SpySabotageEffect SabotageEffect = SpySabotageEffect.None;
}

/// <summary>How a contract is carried out - see SpyContractSystem for the handling of each.</summary>
public enum SpyContractType : byte
{
    /// <summary>Plant the tracker on a static target and hold position nearby for Duration (long DoAfter, breaks on move/damage).</summary>
    Surveillance,

    /// <summary>Apply the tracker to a device for a short DoAfter, then apply SabotageEffect.</summary>
    Sabotage,

    /// <summary>Plant the tracker on a target, then stay within Radius of it (may move) until Duration accumulates.</summary>
    SurveillanceProximity,

    /// <summary>Apply the tracker to a data source for a short "download" DoAfter, no damage to the target.</summary>
    CollectData,

    /// <summary>Kill the target with your own hands. No tracker/timer - completion is detected on the target's death.</summary>
    Assassinate,
}

public enum SpyContractDifficulty : byte
{
    Easy,
    Medium,
    Hard,
}

/// <summary>Effect applied to a Sabotage contract's target on completion.</summary>
public enum SpySabotageEffect : byte
{
    None,

    /// <summary>Drain the target's battery (SMES/APC) to zero.</summary>
    DrainBattery,

    /// <summary>Bolt the target door shut.</summary>
    BoltDoor,

    /// <summary>A short EMP pulse on the target.</summary>
    EmpPulse,

    /// <summary>Heavy structural damage ("break" the device).</summary>
    BreakDevice,
}
