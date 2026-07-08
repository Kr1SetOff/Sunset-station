using System.Linq;
using Content.Shared._Sunset.Spy.SpyContracts;
using Content.Shared.Store;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Sunset.Spy;

/// <summary>
/// Custom BUI for the Spy Uplink - renders with SpyUplinkMenu (a red/black-themed window with a
/// Market tab and a Contracts tab) instead of the default StoreMenu, but still speaks the vanilla
/// Store messages for purchasing/withdrawing (see SpyUplinkUpdateState for why that's safe to reuse).
/// </summary>
[UsedImplicitly]
public sealed class SpyUplinkBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private SpyUplinkMenu? _menu;

    public SpyUplinkBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SpyUplinkMenu>();

        _menu.OnListingButtonPressed += listing => SendMessage(new StoreBuyListingMessage(listing.ID));
        _menu.OnWithdrawAttempt += (currency, amount) => SendMessage(new StoreRequestWithdrawMessage(currency, amount));
        _menu.OnContractAccepted += contractId => SendMessage(new SpyAcceptContractMessage(contractId));
        _menu.OnContractAbandoned += () => SendMessage(new SpyAbandonContractMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is SpyUplinkUpdateState msg)
        {
            _menu?.UpdateBalance(msg.Balance);
            _menu?.UpdateListings(msg.Listings.ToList());
            _menu?.UpdateContracts(msg.Board, msg.Active, msg.Reputation, msg.NextRotationSeconds);
        }
    }
}
