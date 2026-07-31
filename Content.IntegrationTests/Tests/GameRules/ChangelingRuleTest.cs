using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.Server._Goobstation.Changeling.GameTicking.Rules;
using Content.Server.GameTicking;
using Content.Server.Mind;
using Content.Server.Roles;
using Content.Server.Store.Systems;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared.Actions.Components;
using Content.Shared.GameTicking;
using Content.Shared.GameTicking.Components;
using Content.Shared.NPC.Prototypes;
using Content.Shared.NPC.Systems;
using Content.Shared.Store.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests.GameRules;

[TestFixture]
public sealed class ChangelingRuleTest : GameTest
{
    private const string ChangelingGameRuleProtoId = "Changeling";
    private const string ChangelingAntagRoleName = "Changeling";
    private static readonly ProtoId<NpcFactionPrototype> ChangelingFaction = "Changeling";
    private static readonly ProtoId<NpcFactionPrototype> NanotrasenFaction = "NanoTrasen";

    public override PoolSettings PoolSettings => new()
    {
        Dirty = true,
        DummyTicker = false,
        Connected = true,
        InLobby = true,
    };

    /// <summary>
    /// Regression test for the upstream-sync merge (2026-07-30): ChangelingComponent and its starting
    /// evolutions (identity/chemical/regenerate/stasis, and the base actions they grant) were all wired
    /// up via MapInitEvent, which only fires for freshly spawned/map-loaded entities. Since
    /// ChangelingRuleSystem.MakeChangeling calls EnsureComp on an already-playing player's body, none of
    /// that ever ran and the changeling ended up with zero abilities. Fixed by switching those handlers
    /// to ComponentStartup (matching how VampireSystem does the equivalent thing).
    /// </summary>
    [Test]
    public async Task TestChangelingGetsStartingAbilities()
    {
        var pair = Pair;
        var server = pair.Server;
        var protoMan = server.ProtoMan;
        var entMan = server.EntMan;
        var ticker = server.System<GameTicker>();
        var mindSys = server.System<MindSystem>();
        var roleSys = server.System<RoleSystem>();
        var factionSys = server.System<NpcFactionSystem>();

        // The Changeling game rule requires 15 players (see _Goobstation/GameRules/roundstart.yml),
        // otherwise SecretRuleSystem.CanPick-style minPlayers checks fail the rule and the round
        // start bails out entirely.
        var minPlayers = 1;
        await server.WaitAssertion(() =>
        {
            Assert.That(protoMan.TryIndex<EntityPrototype>(ChangelingGameRuleProtoId, out var gameRuleEnt));
            Assert.That(gameRuleEnt!.TryGetComponent<GameRuleComponent>(out var gameRule, server.ResolveDependency<IComponentFactory>()));
            minPlayers = gameRule!.MinPlayers;
        });

        await pair.Server.AddDummySessions(minPlayers - 1);
        await pair.RunTicksSync(5);

        await pair.SetAntagPreferences([ChangelingAntagRoleName]);

        await server.WaitPost(() =>
        {
            var gameRuleEnt = ticker.AddGameRule(ChangelingGameRuleProtoId);
            Assert.That(entMan.TryGetComponent<ChangelingRuleComponent>(gameRuleEnt, out _));

            ticker.ToggleReadyAll(true);
            ticker.StartRound();
            ticker.StartGameRule(gameRuleEnt);
        });
        await pair.RunTicksSync(10);

        Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.InRound));

        var player = pair.Player!.AttachedEntity!.Value;
        Assert.That(entMan.EntityExists(player));

        var mind = mindSys.GetMind(player)!.Value;
        Assert.That(roleSys.MindIsAntagonist(mind));
        Assert.That(factionSys.IsMember(player, ChangelingFaction), Is.True);
        Assert.That(factionSys.IsMember(player, NanotrasenFaction), Is.False);

        // The actual regression check: the changeling must have its identity component, its starting
        // evolutions must be marked assigned, and it must have actually been granted actions - not just
        // the bare marker component with nothing behind it.
        Assert.That(entMan.TryGetComponent<ChangelingComponent>(player, out var changelingComp));
        Assert.That(changelingComp!.EvolutionsAssigned, Is.True,
            "Changeling was never granted its starting evolutions (ChangelingIdentity/Chemical/Regenerate/Stasis)!");
        Assert.That(entMan.HasComponent<ChangelingIdentityComponent>(player),
            "Changeling never got its ChangelingIdentityComponent - starting evolutions didn't run!");

        Assert.That(entMan.TryGetComponent<ActionsComponent>(player, out var actionsComp));
        Assert.That(actionsComp!.Actions, Is.Not.Empty,
            "Changeling has zero granted actions - abilities were not wired up!");

        // Regression check for the PVS crash reported after this fix landed: every changeling
        // evolution's *PurchasedEvent (Content.Shared._Goobstation.Changeling.ChangelingEvents.cs) and
        // every wizard spellbook productEvent (Content.Shared._Goobstation.Wizard.SpellEvents.cs) was
        // missing [Serializable, NetSerializable], so as soon as a store listing carrying one of those
        // as its ProductEvent needed to be sent to a client, PvsSystem.SerializeState threw
        // KeyNotFoundException deep in NetSerializer and the affected player couldn't stay connected.
        // Add the AugmentedEyesight evolution (the one from the bug report) to the live store and run
        // several more ticks so the game state actually gets serialized with it present.
        var storeSys = server.System<StoreSystem>();
        await server.WaitAssertion(() =>
        {
            Assert.That(entMan.TryGetComponent<StoreComponent>(player, out var storeComp));
            Assert.That(storeSys.TryAddListing(storeComp!, "EvolutionMenuUtilityEyesight"), Is.True,
                "Could not add the EvolutionMenuUtilityEyesight listing to the changeling's store!");
        });
        await pair.RunTicksSync(10);
    }
}
