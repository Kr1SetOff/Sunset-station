using Content.Shared.FixedPoint;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Spy.SpyContracts;

/// <summary>
/// Combined state for the custom Spy Uplink BUI: the normal Store balance/listings (rendered
/// with the vanilla StoreListingControl, just inside a themed window), the rotating contract
/// board, the spy's currently active contract (if any), their reputation, and a countdown to the
/// next board rotation. Pushed to Content.Shared.Store.StoreUiKey.Key - see SpyContractSystem for
/// why reusing that key (instead of a second one) lets purchasing keep working through the
/// untouched vanilla StoreSystem handlers.
/// </summary>
[Serializable, NetSerializable]
public sealed class SpyUplinkUpdateState : BoundUserInterfaceState
{
    public readonly HashSet<ListingDataWithCostModifiers> Listings;
    public readonly Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> Balance;
    public readonly List<SpyContractInfo> Board;
    public readonly SpyActiveContractInfo? Active;
    public readonly int Reputation;
    public readonly float NextRotationSeconds;

    public SpyUplinkUpdateState(
        HashSet<ListingDataWithCostModifiers> listings,
        Dictionary<ProtoId<CurrencyPrototype>, FixedPoint2> balance,
        List<SpyContractInfo> board,
        SpyActiveContractInfo? active,
        int reputation,
        float nextRotationSeconds)
    {
        Listings = listings;
        Balance = balance;
        Board = board;
        Active = active;
        Reputation = reputation;
        NextRotationSeconds = nextRotationSeconds;
    }
}

/// <summary>A contract currently offered on the board, not yet accepted.</summary>
[Serializable, NetSerializable]
public sealed class SpyContractInfo
{
    public ProtoId<SpyContractPrototype> Id;

    /// <summary>Reward already adjusted for the spy's current reputation.</summary>
    public int Reward;

    public SpyContractInfo(ProtoId<SpyContractPrototype> id, int reward)
    {
        Id = id;
        Reward = reward;
    }
}

/// <summary>The spy's currently accepted contract.</summary>
[Serializable, NetSerializable]
public sealed class SpyActiveContractInfo
{
    public ProtoId<SpyContractPrototype> Id;

    /// <summary>Reward locked in at accept time.</summary>
    public int Reward;

    /// <summary>0-1 progress, only meaningful for SurveillanceProximity (other types show their own DoAfter bar).</summary>
    public float Progress;

    public SpyActiveContractInfo(ProtoId<SpyContractPrototype> id, int reward, float progress)
    {
        Id = id;
        Reward = reward;
        Progress = progress;
    }
}

/// <summary>Accepts a contract straight from the board - no separate confirm step.</summary>
[Serializable, NetSerializable]
public sealed class SpyAcceptContractMessage : BoundUserInterfaceMessage
{
    public ProtoId<SpyContractPrototype> ContractId;

    public SpyAcceptContractMessage(ProtoId<SpyContractPrototype> contractId)
    {
        ContractId = contractId;
    }
}

/// <summary>Gives up the currently active contract (reputation penalty).</summary>
[Serializable, NetSerializable]
public sealed class SpyAbandonContractMessage : BoundUserInterfaceMessage;
