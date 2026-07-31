using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Pair;
using Content.Server.GameTicking;
using Content.Server.Preferences.Managers;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Magic.Components;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests.GameRules;

/// <summary>
/// Fires every wizard spell action through the real ActionsSystem.PerformAction path and spawns every
/// spellbook item, checking the server doesn't log an error doing either. Companion to
/// ChangelingAbilitiesTest - same rationale: granting an ability isn't the same as it working.
/// </summary>
[TestFixture]
public sealed class WizardAbilitiesTest : GameTest
{
    public override PoolSettings PoolSettings => new()
    {
        Dirty = true,
        DummyTicker = false,
        Connected = true,
        InLobby = true,
    };

    // Every productAction in Resources/Prototypes/_Goobstation/Wizard/spellbook_catalog.yml, minus
    // ActionMutateSpell (destructively re-rolls the caster's whole kit - tested separately, last).
    private static readonly string[] InstantSpells =
    {
        "ActionMagicMissile", "ActionDisableTech", "ActionStopTime", "ActionBindSoul",
        "ActionTeslaBlast", "ActionArcaneBarrage", "ActionLesserSummonGuns", "ActionInstantSummons",
        "ActionTeleportWizard", "ActionTrapsSpell", "ActionSummonSimians", "ActionChuuniInvocations",
        "ActionSoulTap", "ActionThrownLightning", "ActionRathenSpell",
    };

    private static readonly string[] EntityTargetSpells =
    {
        "ActionCluwneCurse", "ActionBananaTouch", "ActionMimeMalaise", "ActionCorpseExplosion",
        "ActionBlindSpell", "ActionLightningBolt", "ActionBarnyardCurse", "ActionScreamForMe",
        "ActionSwapSpell", "ActionTileToggle",
    };

    private static readonly string[] WorldTargetSpells =
    {
        "ActionHomingToolbox", "ActionSpellCards",
    };

    private static readonly string[] SpellbookItems =
    {
        "ClothingBackpackOblivionEnforcerBundle", "ClothingBackpackWizardClownBundle",
        "ClothingBackpackWizardMimeBundle", "ClothingOuterHardsuitWizard", "ClothingOuterWizardPaperReal",
        "ClothingShoesWizardSkates", "ContractApprenticeship", "EchoKatana", "EverfullMug",
        "HighFrequencyBlade", "MagicalLamp", "MultiWandWizard", "ScryingOrbWizard", "SpearGreyTide",
        "Spellblade", "SupermatterHalberd", "ToolboxTiderFilled", "WeaponStaffChange",
        "WeaponStaffChaos", "WeaponStaffSlipping",
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

    private static async Task<EntityUid> SpawnRoundStartedWizard(TestPair pair)
    {
        var server = pair.Server;
        var entMan = server.EntMan;
        var ticker = server.System<GameTicker>();

        // Force Human, same reasoning as ChangelingAbilitiesTest: avoids unrelated species-specific
        // hand/anatomy quirks that have nothing to do with what's being tested here.
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

        var wizard = pair.Player!.AttachedEntity!.Value;

        await server.WaitAssertion(() =>
        {
            var actions = entMan.System<SharedActionsSystem>();
            foreach (var protoId in InstantSpells.Concat(EntityTargetSpells).Concat(WorldTargetSpells))
                actions.AddAction(wizard, protoId);
        });

        return wizard;
    }

    private static void Perform(IEntityManager entMan, EntityUid performer, string actionProtoId, BaseActionEvent? ev = null)
    {
        var actions = entMan.System<SharedActionsSystem>();
        var action = GetAction(entMan, performer, actionProtoId);
        actions.PerformAction((performer, entMan.GetComponent<ActionsComponent>(performer)), (action, entMan.GetComponent<ActionComponent>(action)), ev);
    }

    [Test]
    public async Task TestSpells()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.EntMan;

        var wizard = await SpawnRoundStartedWizard(pair);
        var coords = entMan.GetComponent<TransformComponent>(wizard).Coordinates;

        EntityUid target1 = default, target2 = default, target3 = default, target4 = default, target5 = default;
        await server.WaitPost(() =>
        {
            target1 = entMan.SpawnEntity("MobHuman", coords);
            target2 = entMan.SpawnEntity("MobHuman", coords);
            target3 = entMan.SpawnEntity("MobHuman", coords);
            target4 = entMan.SpawnEntity("MobHuman", coords);
            target5 = entMan.SpawnEntity("MobHuman", coords);
        });
        await pair.RunTicksSync(5);

        await server.WaitPost(() =>
        {
            foreach (var proto in InstantSpells)
                Perform(entMan, wizard, proto);
        });
        await pair.RunTicksSync(20);

        await server.WaitPost(() => Perform(entMan, wizard, "ActionCluwneCurse", new CluwneCurseEvent { Target = target1 }));
        await pair.RunTicksSync(5);
        await server.WaitPost(() => Perform(entMan, wizard, "ActionBananaTouch", new BananaTouchEvent { Target = target1 }));
        await pair.RunTicksSync(5);
        await server.WaitPost(() => Perform(entMan, wizard, "ActionMimeMalaise", new MimeMalaiseEvent { Target = target2 }));
        await pair.RunTicksSync(5);
        await server.WaitPost(() => Perform(entMan, wizard, "ActionBlindSpell", new BlindSpellEvent { Target = target4 }));
        await pair.RunTicksSync(5);
        await server.WaitPost(() => Perform(entMan, wizard, "ActionLightningBolt", new LightningBoltEvent { Target = target4 }));
        await pair.RunTicksSync(5);
        await server.WaitPost(() => Perform(entMan, wizard, "ActionBarnyardCurse", new BarnyardCurseEvent { Target = target5 }));
        await pair.RunTicksSync(5);
        await server.WaitPost(() => Perform(entMan, wizard, "ActionScreamForMe", new ScreamForMeEvent { Target = target5 }));
        await pair.RunTicksSync(5);
        await server.WaitPost(() => Perform(entMan, wizard, "ActionSwapSpell", new SwapSpellEvent { Target = target2 }));
        await pair.RunTicksSync(5);
        await server.WaitPost(() => Perform(entMan, wizard, "ActionTileToggle", new TileToggleSpellEvent { Target = target2 }));
        await pair.RunTicksSync(5);

        // CorpseExplosion consumes/gibs its target - tested last among the entity-target spells.
        await server.WaitPost(() => Perform(entMan, wizard, "ActionCorpseExplosion", new CorpseExplosionEvent { Target = target3 }));
        await pair.RunTicksSync(5);

        await server.WaitPost(() =>
        {
            Perform(entMan, wizard, "ActionHomingToolbox", new HomingToolboxEvent { Target = coords });
            Perform(entMan, wizard, "ActionSpellCards", new SpellCardsEvent { Target = coords });
        });
        await pair.RunTicksSync(10);
    }

    [Test]
    public async Task TestMutateSpell()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.EntMan;

        var wizard = await SpawnRoundStartedWizard(pair);
        await server.WaitAssertion(() => entMan.System<SharedActionsSystem>().AddAction(wizard, "ActionMutateSpell"));
        await pair.RunTicksSync(5);

        await server.WaitPost(() => Perform(entMan, wizard, "ActionMutateSpell"));
        await pair.RunTicksSync(10);
    }

    // Every base action wired to a Level 2 upgrade in spellbook_upgrades.yml.
    private static readonly string[] UpgradableSpells =
    {
        "ActionMimeMalaise", "ActionCluwneCurse", "ActionBananaTouch", "ActionBlindSpell",
        "ActionMutateSpell", "ActionTeslaBlast", "ActionLightningBolt", "ActionHomingToolbox",
        "ActionArcaneBarrage", "ActionLesserSummonGuns", "ActionBarnyardCurse", "ActionScreamForMe",
        "ActionLesserSummonBees", "ActionSanguineStrike", "ActionRathenSpell", "ActionSpellCards",
        "ActionSummonSimians", "ActionMagicMissile", "ActionDisableTech", "ActionStopTime",
        "ActionSwapSpell", "ActionTeleportWizard", "ActionTrapsSpell",
    };

    /// <summary>
    /// Regression test: every one of these 23 upgrades was non-functional (base actions had no
    /// ActionUpgradeComponent at all, so TryUpgradeAction always failed and purchases silently
    /// refunded). Grants each base spell, then calls the same ActionUpgradeSystem.TryUpgradeAction
    /// the store purchase flow calls, and checks the action actually became its Level 2 variant with
    /// requiresClothes turned off.
    /// </summary>
    [Test]
    public async Task TestSpellUpgrades()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.EntMan;

        var wizard = await SpawnRoundStartedWizard(pair);

        await server.WaitAssertion(() =>
        {
            var actions = entMan.System<SharedActionsSystem>();
            var actionUpgrade = entMan.System<ActionUpgradeSystem>();

            using (Assert.EnterMultipleScope())
            {
                foreach (var baseProto in UpgradableSpells)
                {
                    var actionId = actions.AddAction(wizard, baseProto);
                    Assert.That(actionId, Is.Not.Null, $"Could not grant base action '{baseProto}'!");

                    Assert.That(actionUpgrade.TryUpgradeAction(actionId, out var upgradedId), Is.True,
                        $"TryUpgradeAction failed for '{baseProto}' - ActionUpgradeComponent/EffectedLevels missing or broken!");
                    Assert.That(upgradedId, Is.Not.Null);

                    var meta = entMan.GetComponent<MetaDataComponent>(upgradedId!.Value);
                    Assert.That(meta.EntityPrototype?.ID, Is.EqualTo(baseProto + "2"),
                        $"'{baseProto}' upgraded to the wrong prototype (expected {baseProto}2, got {meta.EntityPrototype?.ID})!");

                    Assert.That(entMan.TryGetComponent<MagicComponent>(upgradedId.Value, out var magic), Is.True);
                    Assert.That(magic!.RequiresClothes, Is.False,
                        $"'{baseProto}2' should no longer require wizard robes!");
                }
            }
        });
    }

    [Test]
    public async Task TestSpellbookItemsSpawn()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.EntMan;

        var wizard = await SpawnRoundStartedWizard(pair);
        var coords = entMan.GetComponent<TransformComponent>(wizard).Coordinates;

        await server.WaitAssertion(() =>
        {
            foreach (var proto in SpellbookItems)
            {
                var item = entMan.SpawnEntity(proto, coords);
                Assert.That(entMan.EntityExists(item), $"Failed to spawn spellbook item '{proto}'!");
            }
        });
    }
}
