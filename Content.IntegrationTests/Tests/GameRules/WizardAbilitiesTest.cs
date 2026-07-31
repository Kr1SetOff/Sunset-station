using System.Linq;
using Content.IntegrationTests.Fixtures;
using Content.IntegrationTests.Pair;
using Content.Server.GameTicking;
using Content.Server.Preferences.Managers;
using Content.Shared._Goobstation.Wizard;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Inventory;
using Content.Shared.Magic.Components;
using Content.Shared.Preferences;
using Robust.Shared.GameObjects;
using Robust.Shared.Localization;
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
            // Every base spell action has Magic.RequiresClothes = true, which SharedMagicSystem's
            // BeforeCastSpellEvent gate silently cancels unless the caster is wearing a
            // WizardClothesComponent hat and outer layer. Without this, every Perform() call below
            // would cancel before the spell's own handler ever runs - the test would still pass
            // (PerformAction doesn't throw for a cancelled cast), but nothing would actually be
            // exercised. A real Wizard antag starts equipped with these; this stands in for that.
            var inventory = entMan.System<InventorySystem>();
            inventory.TryEquip(wizard, entMan.SpawnEntity("ClothingOuterWizard", entMan.GetComponent<TransformComponent>(wizard).Coordinates), "outerClothing", true, true);
            inventory.TryEquip(wizard, entMan.SpawnEntity("ClothingHeadHatWizard", entMan.GetComponent<TransformComponent>(wizard).Coordinates), "head", true, true);

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

    /// <summary>
    /// Regression test: Rathen's Curse looked up nearby targets by FartComponent - the very component
    /// the spell is supposed to grant on the first hit, so nobody ever had it and the shockwave could
    /// never find a single target. It now grants the component to whatever it hits instead.
    /// </summary>
    [Test]
    public async Task TestRathenSpell()
    {
        var pair = Pair;
        var server = pair.Server;
        var entMan = server.EntMan;

        var wizard = await SpawnRoundStartedWizard(pair);
        var coords = entMan.GetComponent<TransformComponent>(wizard).Coordinates;

        EntityUid target = default;
        await server.WaitPost(() => target = entMan.SpawnEntity("MobHuman", coords));
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() => entMan.System<SharedActionsSystem>().AddAction(wizard, "ActionRathenSpell"));
        await pair.RunTicksSync(5);

        await server.WaitPost(() => Perform(entMan, wizard, "ActionRathenSpell"));
        await pair.RunTicksSync(10);

        await server.WaitAssertion(() =>
        {
            Assert.That(entMan.TryGetComponent<FartComponent>(target, out var fart), Is.True,
                "Rathen's Curse didn't affect the nearby target - the shockwave found nobody to hit!");
            Assert.That(fart!.SuperFarted, Is.True);
        });
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

    // Every popup/examine/verb key referenced by the wizard spell and item C# code that had no
    // matching locale entry at all (in either language) until this was added - the player would see
    // the raw untranslated key printed on screen instead of a message. Regression guard: these calls
    // are scattered across a dozen files and don't run in TestSpells (most only fire on a failure
    // path), so a missing key wouldn't otherwise be caught by any other test.
    private static readonly string[] WizardFeedbackLocaleKeys =
    {
        "spell-fail-no-targets", "spell-fail-target-borg", "spell-fail-not-dead",
        "spell-fail-soul-not-bound", "spell-fail-item-destroyed", "spell-fail-item-on-another-plane",
        "spell-fail-no-soul", "spell-fail-bind-soul-silicon", "spell-fail-no-held-entity",
        "spell-fail-unremoveable", "spell-fail-soul-item-not-suitable", "spell-fail-mutate-silicon",
        "spell-fail-lightning-bolt", "spell-fail-target-cant-wear-mask", "spell-fail-target-cursed",
        "spell-fail-target-silicon", "spell-fail-cant-wear-eyepatch", "spell-fail-already-wear-eyepatch",
        "spell-fail-sanguine-strike-no-item", "spell-fail-sanguine-strike-already-empowered",
        "spell-fail-sanguine-strike-not-weapon", "spell-fail-hands-occupied", "spell-fail-tesla-blast",
        "spell-fail-no-spells", "spell-requirements-failed",
        "spell-soul-tap-message", "spell-soul-tap-almost-dead-message", "spell-soul-tap-dead-message-user",
        "spell-soul-tap-dead-message-others",
        "spell-charge-spells-charged-entity", "spell-charge-spells-charged-pulled",
        "spell-charge-no-spells-to-charge-pulled",
        "spell-rathen-fart-popup", "spell-rathen-gut-popup",
        "spell-summon-simians-maxed-out-message", "instant-summons-item-marked", "lich-greeting",
        "blink-activated-message", "blink-deactivated-message",
        "chuuni-eyepatch-backstory-1", "chuuni-eyepatch-backstory-2", "chuuni-eyepatch-backstory-3",
        "chuuni-eyepatch-backstory-4",
        "enchanted-rifle-guns-left", "ensouled-item-desc", "ensouled-item-name",
        "hulk-roar-1", "hulk-roar-2", "hulk-roar-3", "hulk-roar-4", "hulk-roar-5",
        "ice-cube-break-free-start", "sanguine-strike-examine",
        "scrying-orb-verb-message", "scrying-orb-verb-text", "spellblade-examine-enchantment",
        "teleport-scroll-no-charges", "teleport-scroll-uses-left",
        "trap-triggered-message", "trap-revealed-message", "trap-flare-message",
        "wizard-mirror-guardian-change-species-fail",
        "chat-emote-name-fart-super", "chat-emote-msg-fart-super",
        "alerts-hierophant-beat-name", "alerts-hierophant-beat-desc",
    };

    [Test]
    public void TestSpellFeedbackLocalization()
    {
        var server = Pair.Server;
        var loc = server.ResolveDependency<ILocalizationManager>();

        using (Assert.EnterMultipleScope())
        {
            foreach (var key in WizardFeedbackLocaleKeys)
                Assert.That(loc.HasString(key), Is.True, $"Missing locale string for wizard key '{key}'!");
        }
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
