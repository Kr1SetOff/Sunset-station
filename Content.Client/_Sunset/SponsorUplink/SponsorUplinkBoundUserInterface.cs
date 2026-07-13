using System.Linq;
using Content.Shared.Store;
using JetBrains.Annotations;
using Robust.Client.UserInterface;

namespace Content.Client._Sunset.SponsorUplink;

/// <summary>
/// Custom BUI for the Sponsor Uplink - renders with SponsorUplinkMenu (a purple/orange card-grid
/// window) instead of the default StoreMenu, but still speaks the vanilla Store messages for
/// purchasing/withdrawing, since the server side is just a plain StoreComponent.
/// </summary>
[UsedImplicitly]
public sealed class SponsorUplinkBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private SponsorUplinkMenu? _menu;

    public SponsorUplinkBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<SponsorUplinkMenu>();

        _menu.OnListingButtonPressed += listing => SendMessage(new StoreBuyListingMessage(listing.ID));
        _menu.OnWithdrawAttempt += (currency, amount) => SendMessage(new StoreRequestWithdrawMessage(currency, amount));
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is StoreUpdateState msg)
        {
            _menu?.UpdateBalance(msg.Balance);
            _menu?.UpdateListings(msg.Listings.ToList());
        }
    }
}
