using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.Server._Goobstation.Changeling;
using Content.Server._Goobstation.Changeling.GameTicking.Rules;
using Content.Server.GameTicking;
using Content.IntegrationTests.Pair;
using Content.Shared._Goobstation.Changeling.Actions;
using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.InternalResources.EntitySystems;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Server.Player;
using Content.Server.Preferences.Managers;

namespace Content.IntegrationTests.Tests.GameRules;

/// <summary>
/// Fires every Changeling ability event through the real ActionsSystem.PerformAction path (the same
/// path a player click takes) and checks the server doesn't log an error doing it. This is a
/// regression test for the 2026-07-30 upstream-sync merge, which left several Changeling abilities
/// silently non-functional (ComponentStartup/MapInitEvent mixups, a wrong audio path, missing
/// [NetSerializable] on store events) that ChangelingRuleTest alone didn't catch because it only
/// checks that abilities were *granted*, not that using them actually works.
/// </summary>
[TestFixture]
public sealed class ChangelingAbilitiesTest : GameTest
{
    public override PoolSettings PoolSettings => new()
    {
        Dirty = true,
        DummyTicker = false,
        Connected = true,
        InLobby = true,
    };

    private static EntityUid GetAction(IEntityManager entMan, EntityUid performer, string protoId)
    {
        var actionsComp = entMan.GetComponent<ActionsComponent>(performer);
        foreach (var action in actionsComp.Actions)
        {
            var meta = entMan.GetComponent<MetaDataComponent>(action);
            if (meta.EntityPrototype?.ID == protoId)
                return action;
        }

        Assert.Fail($"{performer} was not granted an action with prototype id '{protoId}'!");
        return default;
    }

    /// <summary>
    /// Actions only actually land in a body's ActionsComponent for a real round-spawned, session-backed
    /// player entity - a bare SpawnEntity + manually created/transferred Mind (no session) silently
    /// grants none of them. Starts the round proper and turns the connected test player into a
    /// changeling via the real antag-assignment path (ChangelingRuleSystem.MakeChangeling), same as
    /// ChangelingRuleTest.
    /// </summary>
    private static async Task<EntityUid> SpawnRoundStartedChangeling(TestPair pair)
    {
        var server = pair.Server;
        var entMan = server.EntMan;
        var ticker = server.System<GameTicker>();

        // Force the test player to Human: the round-start default profile can randomly roll a
        // non-standard species (e.g. Arachnid), whose hand/anatomy setup trips unrelated
        // pre-existing bugs in hand-item removal that have nothing to do with Changeling.
        var prefMan = server.ResolveDependency<IServerPreferencesManager>();
        var userId = pair.Player!.UserId;
        var prefs = prefMan.GetPreferences(userId);
        var profile = ((HumanoidCharacterProfile)prefs.Characters[0]).WithSpecies("Human");
        await server.WaitPost(() => prefMan.SetProfile(userId, 0, profile).Wait());

        await server.WaitPost(() =>
        {
            ticker.ToggleReadyAll(true);
            ticker.StartRound();
        });
        await pair.RunTicksSync(10);

        var ling = pair.Player!.AttachedEntity!.Value;

        await server.WaitAssertion(() =>
        {
            var ruleSys = entMan.System<ChangelingRuleSystem>();
            Assert.That(ruleSys.MakeChangeling(ling, new ChangelingRuleComponent()), Is.True,
                "ChangelingRuleSystem.MakeChangeling failed for the connected test player!");

            Assert.That(entMan.TryGetComponent<ChangelingComponent>(ling, out var changelingComp), "no ChangelingComponent");
            Assert.That(changelingComp!.EvolutionsAssigned, Is.True, "EvolutionsAssigned is false");
            Assert.That(entMan.HasComponent<ChangelingIdentityComponent>(ling), "no ChangelingIdentityComponent");
            Assert.That(entMan.TryGetComponent<ActionsComponent>(ling, out var actionsComp), "no ActionsComponent");
            Assert.That(actionsComp!.Actions, Is.Not.Empty, "ActionsComponent.Actions is empty");

            GrantAllPurchasableActions(entMan, ling);
        });

        return ling;
    }

    private static void TopUpResources(IEntityManager entMan, EntityUid uid)
    {
        var resources = entMan.System<SharedInternalResourcesSystem>();
        resources.TryUpdateResourcesAmount(uid, "ChangelingChemicals", 100000f);
        resources.TryUpdateResourcesAmount(uid, "ChangelingBiomass", 100000f);
    }

    // Only the 5 BaseChangelingActions (see ChangelingIdentityComponent) are granted automatically on
    // becoming a changeling - everything else here is normally bought with evolution points from the
    // store. Grant them all directly so every ability can actually be exercised.
    private static readonly string[] PurchasableActions =
    {
        "ActionToggleArmblade", "ActionToggleHammer", "ActionToggleClaw", "ActionToggleDartGun",
        "ActionCreateBoneShard", "ActionToggleChitinousArmor", "ActionToggleOrganicShield",
        "ActionShriekDissonant", "ActionShriekResonant", "ActionToggleStrainedMuscles",
        "ActionStingBlind", "ActionStingCryo", "ActionStingLethargic", "ActionStingMute",
        "ActionStingFakeArmblade", "ActionStingTransform", "ActionLayEgg",
        "ActionAnatomicPanacea", "ActionAugmentedEyesight", "ActionBiodegrade", "ActionChameleonSkin",
        "ActionDarknessAdaption", "ActionAdrenalineReserves", "ActionFleshmend", "ActionLastResort",
        "ActionToggleLesserForm", "ActionHivemindAccess", "ActionAbsorbBiomatter",
    };

    private static void GrantAllPurchasableActions(IEntityManager entMan, EntityUid uid)
    {
        var actions = entMan.System<SharedActionsSystem>();
        foreach (var protoId in PurchasableActions)
            actions.AddAction(uid, protoId);
    }

    private static void Perform(IEntityManager entMan, EntityUid performer, string actionProtoId, BaseActionEvent? ev = null)
    {
        var actions = entMan.System<SharedActionsSystem>();
        var action = GetAction(entMan, performer, actionProtoId);
        actions.PerformAction((performer, entMan.GetComponent<ActionsComponent>(performer)), (action, entMan.GetComponent<ActionComponent>(action)), ev);
    }

    /// <summary>
    /// Every ability that doesn't need a target and doesn't end the changeling's current body
    /// (no transform/last-resort/lay-egg), plus DNA absorption/transformation and the sting kit
    /// against fresh humanoid targets.
    /// </summary>
    [Test]
    public async Task TestNonTerminalAbilities()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.EntMan;
        var mobState = server.System<MobStateSystem>();

        var ling = await SpawnRoundStartedChangeling(pair);

        EntityUid absorbTarget = default;
        EntityUid stingTarget1 = default;
        EntityUid stingTarget2 = default;
        EntityUid stingTarget3 = default;
        EntityUid biomatterTarget = default;

        await server.WaitPost(() =>
        {
            TopUpResources(entMan, ling);

            // Targets need to be colocated with the performer, not in nullspace on a different map -
            // AbsorbDNA's DoAfter has a DistanceThreshold check that never resolves across maps.
            var coords = entMan.GetComponent<TransformComponent>(ling).Coordinates;

            absorbTarget = entMan.SpawnEntity("MobHuman", coords);
            mobState.ChangeMobState(absorbTarget, MobState.Dead);

            stingTarget1 = entMan.SpawnEntity("MobHuman", coords);
            stingTarget2 = entMan.SpawnEntity("MobHuman", coords);
            stingTarget3 = entMan.SpawnEntity("MobHuman", coords);

            biomatterTarget = entMan.SpawnEntity("FoodMeat", coords);
        });
        await pair.RunTicksSync(5);

        // --- Basic / utility / combat abilities: no target needed ---
        await server.WaitPost(() =>
        {
            Perform(entMan, ling, "ActionEvolutionMenu");
            Perform(entMan, ling, "ActionChangelingTransformCycle");
            Perform(entMan, ling, "ActionToggleArmblade");
            Perform(entMan, ling, "ActionToggleArmblade"); // toggle back off
            Perform(entMan, ling, "ActionToggleHammer");
            Perform(entMan, ling, "ActionToggleHammer");
            Perform(entMan, ling, "ActionToggleClaw");
            Perform(entMan, ling, "ActionToggleClaw");
            Perform(entMan, ling, "ActionToggleDartGun");
            Perform(entMan, ling, "ActionToggleDartGun");
            Perform(entMan, ling, "ActionCreateBoneShard");
            Perform(entMan, ling, "ActionToggleChitinousArmor");
            Perform(entMan, ling, "ActionToggleChitinousArmor");
            Perform(entMan, ling, "ActionToggleOrganicShield");
            Perform(entMan, ling, "ActionToggleOrganicShield");
            Perform(entMan, ling, "ActionShriekDissonant");
            Perform(entMan, ling, "ActionShriekResonant");
            Perform(entMan, ling, "ActionToggleStrainedMuscles");
            Perform(entMan, ling, "ActionToggleStrainedMuscles");
            Perform(entMan, ling, "ActionAnatomicPanacea");
            Perform(entMan, ling, "ActionBiodegrade");
            Perform(entMan, ling, "ActionAdrenalineReserves");
            Perform(entMan, ling, "ActionFleshmend");
            Perform(entMan, ling, "ActionHivemindAccess");
        });
        await pair.RunTicksSync(10);

        // --- Absorb DNA (DoAfter-gated) then transform using the absorbed identity ---
        await server.WaitPost(() =>
        {
            var action = GetAction(entMan, ling, "ActionAbsorbDNA");
            var ev = new AbsorbDNAEvent { Target = absorbTarget };
            server.System<SharedActionsSystem>()
                .PerformAction((ling, entMan.GetComponent<ActionsComponent>(ling)), (action, entMan.GetComponent<ActionComponent>(action)), ev);
        });
        // AbsorbDNA's DoAfter is 15s @ 30 tick/s = 450 ticks; give it plenty of headroom.
        await pair.RunTicksSync(500);

        await server.WaitAssertion(() =>
        {
            var identity = entMan.GetComponent<ChangelingIdentityComponent>(ling);
            Assert.That(identity.AbsorbedDNA, Is.Not.Empty, "AbsorbDNA's DoAfter never completed / TryStealDNA never ran!");
        });

        await server.WaitPost(() =>
        {
            Perform(entMan, ling, "ActionChangelingTransformCycle");
            Perform(entMan, ling, "ActionChangelingTransform");
        });
        await pair.RunTicksSync(10);

        // --- Stings against fresh, healthy targets (TrySting doesn't require incapacitation) ---
        await server.WaitPost(() =>
        {
            Perform(entMan, ling, "ActionStingExtractDNA", new StingExtractDNAEvent { Target = stingTarget1 });
        });
        await pair.RunTicksSync(5);

        foreach (var stingProto in new[] { "ActionStingBlind", "ActionStingCryo", "ActionStingLethargic", "ActionStingMute" })
        {
            await server.WaitPost(() =>
            {
                Perform(entMan, ling, stingProto, new StingReagentEvent { Target = stingTarget2 });
            });
            await pair.RunTicksSync(5);
        }

        await server.WaitPost(() =>
        {
            Perform(entMan, ling, "ActionStingFakeArmblade", new StingFakeArmbladeEvent { Target = stingTarget2 });
        });
        await pair.RunTicksSync(5);

        await server.WaitPost(() =>
        {
            Perform(entMan, ling, "ActionStingTransform", new StingTransformEvent { Target = stingTarget3 });
        });
        await pair.RunTicksSync(5);

        // --- Absorb biomatter (DoAfter-gated) ---
        await server.WaitPost(() =>
        {
            Perform(entMan, ling, "ActionAbsorbBiomatter", new AbsorbBiomatterEvent { Target = biomatterTarget });
        });
        await pair.RunTicksSync(200);
    }

    [Test]
    public async Task TestLastResort()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.EntMan;

        var ling = await SpawnRoundStartedChangeling(pair);
        await server.WaitPost(() => TopUpResources(entMan, ling));
        await pair.RunTicksSync(5);

        await server.WaitPost(() => Perform(entMan, ling, "ActionLastResort"));
        await pair.RunTicksSync(10);
    }

    [Test]
    public async Task TestLesserForm()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.EntMan;

        var ling = await SpawnRoundStartedChangeling(pair);
        await server.WaitPost(() => TopUpResources(entMan, ling));
        await pair.RunTicksSync(5);

        await server.WaitPost(() => Perform(entMan, ling, "ActionToggleLesserForm"));
        await pair.RunTicksSync(10);
    }

    [Test]
    public async Task TestLayEgg()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.EntMan;
        var mobState = server.System<MobStateSystem>();

        var ling = await SpawnRoundStartedChangeling(pair);

        EntityUid target = default;
        await server.WaitPost(() =>
        {
            TopUpResources(entMan, ling);

            target = entMan.SpawnEntity("MobHuman", MapCoordinates.Nullspace);
            mobState.ChangeMobState(target, MobState.Dead);
        });
        await pair.RunTicksSync(5);

        await server.WaitPost(() =>
        {
            Perform(entMan, ling, "ActionLayEgg", new StingLayEggsEvent { Target = target });
        });
        await pair.RunTicksSync(10);
    }
}
