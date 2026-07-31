using Content.IntegrationTests.Fixtures;
using Content.Shared.Store;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.IntegrationTests.Tests.Store;

[TestFixture]
public sealed class ListingProductEventSerializableTest : GameTest
{
    /// <summary>
    /// ListingData.ProductEvent is serialized generically as an object as part of the store's networked
    /// state (StoreUpdateState), which means every concrete type ever assigned to it via a listing's
    /// `productEvent:` YAML field needs [Serializable, NetSerializable] - RobustSerializer only registers
    /// types carrying [NetSerializable] in NetSerializer's type dictionary, and [DataDefinition] (needed
    /// for the YAML !type: tag to resolve at all) doesn't imply it. Missing the attribute doesn't fail to
    /// load - it throws deep inside PvsSystem.SerializeState the first time a client needs game state
    /// that includes the listing, which is what happened for the Changeling's AugmentedEyesightPurchasedEvent
    /// (and eight other Changeling/Wizard listings) after the 2026-07-30 upstream sync.
    /// </summary>
    [Test]
    public async Task AllListingProductEventsAreNetSerializable()
    {
        var pair = Pair;
        var server = pair.Server;
        var protoMan = server.ProtoMan;

        await server.WaitAssertion(() =>
        {
            using (Assert.EnterMultipleScope())
            {
                foreach (var listing in protoMan.EnumeratePrototypes<ListingPrototype>())
                {
                    if (listing.ProductEvent is not { } productEvent)
                        continue;

                    var type = productEvent.GetType();
                    Assert.That(type.IsDefined(typeof(SerializableAttribute), false)
                                && type.IsDefined(typeof(NetSerializableAttribute), false),
                        $"Listing '{listing.ID}' has a productEvent of type {type.Name} which is missing " +
                        "[Serializable, NetSerializable] - sending this listing's state to a client will crash PvsSystem.SerializeState.");
                }
            }
        });
    }
}
