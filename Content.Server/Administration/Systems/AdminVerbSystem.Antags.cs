using Content.Server._Starlight.Administration.Systems;
using Content.Server._Starlight.GameTicking.Rules.Components;
using Content.Server._Sunset.Cult;
using Content.Server._Sunset.Homelander;
using Content.Server._Sunset.Ratvar;
using Content.Server._Sunset.Spy;
using Content.Server._Sunset.TheBoys.Components;
using Content.Shared._Sunset.Homelander;
using Content.Server.Antag;
using Content.Server.Clothing.Systems;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules.Components;
using Content.Server._Sunset.MalfAi;
using Content.Server._Sunset.Voidwalker;
using Content.Server.Polymorph.Systems;
using Content.Server.Speech.Components; // Starlight
using Content.Server._Starlight.GameTicking.Rules.Components; // Starlight
using Content.Server.Zombies;
using Content.Shared._Starlight.Shadekin;
using Content.Shared.Administration;
using Content.Shared.Database;
using Content.Shared.Humanoid;
using Content.Shared.Mind.Components;
using Content.Shared.Polymorph;
using Content.Shared.Silicons.StationAi;
using Content.Shared.Roles;
using Content.Shared.Roles.Components;
using Content.Shared.Verbs;
using Robust.Shared.Audio; // Starlight
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;
using Content.Shared.Roles.Components;
using Content.Shared._Starlight.Shadekin;
using Content.Server.Speech.Components; // Starlight
using Robust.Shared.Audio; // Starlight

namespace Content.Server.Administration.Systems;

public sealed partial class AdminVerbSystem
{
    [Dependency] private AntagSelectionSystem _antag = default!;
    [Dependency] private ZombieSystem _zombie = default!;
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private OutfitSystem _outfit = default!;
    [Dependency] private AutoDiscordLogSystem _autolog = default!; //Starlight
    [Dependency] private VoidwalkerSpawnRuleSystem _voidwalkerSpawn = default!; // Sunset

    private static readonly EntProtoId DefaultTraitorRule = "Traitor";
    private static readonly EntProtoId DefaultInitialInfectedRule = "Zombie";
    private static readonly EntProtoId DefaultNukeOpRule = "LoneOpsSpawn";
    private static readonly EntProtoId DefaultRevsRule = "Revolutionary";
    private static readonly EntProtoId DefaultThiefRule = "Thief";
    private static readonly EntProtoId DefaultChangelingRule = "Changeling";
    private static readonly EntProtoId DefaultSpyRule = "Spy"; // Sunset
    private static readonly EntProtoId DefaultHomelanderRule = "Homelander"; // Sunset
    private static readonly EntProtoId DefaultTheBoysTeamRule = "TheBoysTeam"; // Sunset
    private static readonly EntProtoId ParadoxCloneRuleId = "ParadoxCloneSpawn";
    private static readonly EntProtoId DefaultWizardRule = "Wizard";
    private static readonly EntProtoId DefaultNinjaRule = "NinjaSpawn";
    private static readonly ProtoId<StartingGearPrototype> PirateGearId = "PirateGear";
    private static readonly EntProtoId DefaultMalfAiRule = "MalfAi"; // Sunset
    private static readonly EntProtoId DefaultVampireRule = "Vampire"; //Starlight
    private static readonly EntProtoId DefaultDevilRule = "Devil"; // starlight
    private static readonly EntProtoId DefaultBrighteyeRule = "Brighteye"; //Starlight
	private static readonly EntProtoId DefaultSELFRule = "SiliconLiberation"; //Starlight
    private static readonly EntProtoId DefaultRatvarCultRule = "RatvarCult"; // Sunset
    private static readonly EntProtoId DefaultRatvarServantRule = "RatvarServantRule"; // Sunset
    private static readonly ProtoId<PolymorphPrototype> VoidwalkerPolymorphId = "VoidwalkerPolymorph"; // Sunset

    // All antag verbs have names so invokeverb works.
    private void AddAntagVerbs(GetVerbsEvent<Verb> args)
    {
        if (!TryComp<ActorComponent>(args.User, out var actor))
            return;

        var player = actor.PlayerSession;

        if (!_adminManager.HasAdminFlag(player, AdminFlags.Fun))
            return;

        if (!HasComp<MindContainerComponent>(args.Target) || !TryComp<ActorComponent>(args.Target, out var targetActor))
            return;

        var targetPlayer = targetActor.PlayerSession;

        var traitorName = Loc.GetString("admin-verb-text-make-traitor");
        Verb traitor = new()
        {
            Text = traitorName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Interface/Misc/job_icons.rsi"), "Syndicate"),
            Act = () =>
            {
                _antag.ForceMakeAntag<TraitorRuleComponent>(targetPlayer, DefaultTraitorRule);
                _autolog.LogToDiscord(string.Join(": ", traitorName, Loc.GetString("admin-verb-make-traitor")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", traitorName, Loc.GetString("admin-verb-make-traitor")),
        };
        args.Verbs.Add(traitor);

        var initialInfectedName = Loc.GetString("admin-verb-text-make-initial-infected");
        Verb initialInfected = new()
        {
            Text = initialInfectedName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "InitialInfected"),
            Act = () =>
            {
                _antag.ForceMakeAntag<ZombieRuleComponent>(targetPlayer, DefaultInitialInfectedRule);
                _autolog.LogToDiscord(string.Join(": ", initialInfectedName, Loc.GetString("admin-verb-make-initial-infected")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", initialInfectedName, Loc.GetString("admin-verb-make-initial-infected")),
        };
        args.Verbs.Add(initialInfected);

        var zombieName = Loc.GetString("admin-verb-text-make-zombie");
        Verb zombie = new()
        {
            Text = zombieName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "Zombie"),
            Act = () =>
            {
                _zombie.ZombifyEntity(args.Target);
                _autolog.LogToDiscord(string.Join(": ", zombieName, Loc.GetString("admin-verb-make-zombie")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", zombieName, Loc.GetString("admin-verb-make-zombie")),
        };
        args.Verbs.Add(zombie);

        // Sunset - Voidwalker. A body-swap polymorph, not a mind-role antag - Voidwalker has its own
        // dedicated body/sprite/stats (see VoidwalkerPolymorph), so it works like Make Zombie above
        // rather than the ForceMakeAntag verbs, which just tag the target's existing body.
        var voidwalkerName = Loc.GetString("admin-verb-text-make-voidwalker");
        Verb voidwalker = new()
        {
            Text = voidwalkerName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Sunset/Mobs/Voidwalker/voidwalker.rsi"), "voidwalker"),
            Act = () =>
            {
                if (_polymorphSystem.PolymorphEntity(args.Target, VoidwalkerPolymorphId) is { } newVoidwalker)
                    _voidwalkerSpawn.PlaceInSpaceNearStation(newVoidwalker);

                _autolog.LogToDiscord(string.Join(": ", voidwalkerName, Loc.GetString("admin-verb-make-voidwalker")), player.Name);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", voidwalkerName, Loc.GetString("admin-verb-make-voidwalker")),
        };
        args.Verbs.Add(voidwalker);

        var nukeOpName = Loc.GetString("admin-verb-text-make-nuclear-operative");
        Verb nukeOp = new()
        {
            Text = nukeOpName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hardsuits/syndicate.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<NukeopsRuleComponent>(targetPlayer, DefaultNukeOpRule);
                _autolog.LogToDiscord(string.Join(": ", nukeOpName, Loc.GetString("admin-verb-make-nuclear-operative")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", nukeOpName, Loc.GetString("admin-verb-make-nuclear-operative")),
        };
        args.Verbs.Add(nukeOp);

        var pirateName = Loc.GetString("admin-verb-text-make-pirate") + " (Wizden)"; // Starlight
        Verb pirate = new()
        {
            Text = pirateName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Clothing/Head/Hats/pirate.rsi"), "icon"),
            Act = () =>
            {
                // pirates just get an outfit because they don't really have logic associated with them
                _outfit.SetOutfit(args.Target, PirateGearId);
                _autolog.LogToDiscord(string.Join(": ", pirateName, Loc.GetString("admin-verb-make-pirate")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", pirateName, Loc.GetString("admin-verb-make-pirate")),
        };
        args.Verbs.Add(pirate);

        var headRevName = Loc.GetString("admin-verb-text-make-head-rev");
        Verb headRev = new()
        {
            Text = headRevName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "HeadRevolutionary"),
            Act = () =>
            {
                _antag.ForceMakeAntag<RevolutionaryRuleComponent>(targetPlayer, DefaultRevsRule);
                _autolog.LogToDiscord(string.Join(": ", headRevName, Loc.GetString("admin-verb-make-head-rev")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", headRevName, Loc.GetString("admin-verb-make-head-rev")),
        };
        args.Verbs.Add(headRev);

        var thiefName = Loc.GetString("admin-verb-text-make-thief");
        Verb thief = new()
        {
            Text = thiefName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Clothing/Hands/Gloves/Color/black.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<ThiefRuleComponent>(targetPlayer, DefaultThiefRule);
                _autolog.LogToDiscord(string.Join(": ", thiefName, Loc.GetString("admin-verb-make-thief")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", thiefName, Loc.GetString("admin-verb-make-thief")),
        };
        args.Verbs.Add(thief);

        // Sunset - Spy antag
        var spyName = Loc.GetString("admin-verb-text-make-spy");
        Verb spy = new()
        {
            Text = spyName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Sunset/Spy_Sprites/spy_uplink.rsi"), "spy_uplink1"),
            Act = () =>
            {
                _antag.ForceMakeAntag<SpyRuleComponent>(targetPlayer, DefaultSpyRule);
                _autolog.LogToDiscord(string.Join(": ", spyName, Loc.GetString("admin-verb-make-spy")), player.Name);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", spyName, Loc.GetString("admin-verb-make-spy")),
        };
        args.Verbs.Add(spy);

        // Sunset - Homelander antag
        var homelanderName = Loc.GetString("admin-verb-text-make-homelander");
        Verb homelander = new()
        {
            Text = homelanderName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Magic/magicactions.rsi"), "fireball"),
            Act = () =>
            {
                _antag.ForceMakeAntag<HomelanderRuleComponent>(targetPlayer, DefaultHomelanderRule);
                _autolog.LogToDiscord(string.Join(": ", homelanderName, Loc.GetString("admin-verb-make-homelander")), player.Name);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", homelanderName, Loc.GetString("admin-verb-make-homelander")),
        };
        args.Verbs.Add(homelander);

        // Sunset - The Boys team antags (Butcher/Frenchie/Hughie/Mother's Milk/Kimiko). Manual
        // fallback for when the automatic per-round pick (TheBoysRuleSystem.AssignTeam) doesn't have
        // enough eligible candidates, or an admin just wants to hand-cast specific players. Reuses
        // the same TheBoysTeam rule entity (creating it if this round isn't running TheBoys at all)
        // and the exact same MakeAntag call the automatic path uses, so briefings/mind roles/Butcher's
        // kill-Homelander objective all still wire up correctly via TheBoysRuleSystem.OnMemberSelected.
        AddTheBoysTeamVerb(args, targetPlayer, player, 0, "admin-verb-text-make-theboys-butcher", "admin-verb-make-theboys-butcher");
        AddTheBoysTeamVerb(args, targetPlayer, player, 1, "admin-verb-text-make-theboys-frenchie", "admin-verb-make-theboys-frenchie");
        AddTheBoysTeamVerb(args, targetPlayer, player, 2, "admin-verb-text-make-theboys-hughie", "admin-verb-make-theboys-hughie");
        AddTheBoysTeamVerb(args, targetPlayer, player, 3, "admin-verb-text-make-theboys-mothersmilk", "admin-verb-make-theboys-mothersmilk");
        AddTheBoysTeamVerb(args, targetPlayer, player, 4, "admin-verb-text-make-theboys-kimiko", "admin-verb-make-theboys-kimiko");

        // Sunset - Ratvar cult. Definitions[0] is the head cultist, Definitions[1] is a rank-and-file
        // cultist (see Resources/Prototypes/_Sunset/Cult/roundstart.yml) - indexed the same way
        // AddTheBoysTeamVerb picks a specific team role instead of letting AntagSelection auto-fill.
        AddRatvarCultVerb(args, targetPlayer, player, 0,
            "admin-verb-text-make-ratvar-cult-leader",
            "admin-verb-make-ratvar-cult-leader",
            "Clothing/Head/Helmets/cult.rsi");
        AddRatvarCultVerb(args, targetPlayer, player, 1,
            "admin-verb-text-make-ratvar-cultist",
            "admin-verb-make-ratvar-cultist",
            "Objects/Weapons/Melee/cult_dagger.rsi");

        // Sunset - Servants of Ratvar (the real clockwork-cult antagonist, distinct from the Nar'Sie
        // cult above). Only one AntagSelection definition exists (no leader split), so this is a plain
        // ForceMakeAntag call like Traitor/Zombie, not the indexed-definition AddRatvarCultVerb pattern.
        var ratvarServantName = Loc.GetString("admin-verb-text-make-ratvar-servant");
        Verb ratvarServant = new()
        {
            Text = ratvarServantName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Sunset/Ratvar/clockwork_slab.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<RatvarServantRuleComponent>(targetPlayer, DefaultRatvarServantRule);
                _autolog.LogToDiscord(string.Join(": ", ratvarServantName, Loc.GetString("admin-verb-make-ratvar-servant")), player.Name);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", ratvarServantName, Loc.GetString("admin-verb-make-ratvar-servant")),
        };
        args.Verbs.Add(ratvarServant);

        // Sunset - remove antagonist role(s), regardless of which antag it is. Logged the same way as
        // granting an antag role, so it's clear from the admin logs / Discord log who de-antagged whom.
        if (_mindSystem.TryGetMind(args.Target, out var targetMindId, out _) &&
            _role.MindIsAntagonist(targetMindId))
        {
            var removeAntagName = Loc.GetString("admin-verb-text-remove-antag");
            Verb removeAntag = new()
            {
                Text = removeAntagName,
                Category = VerbCategory.Antag,
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/close.svg.192dpi.png")),
                Act = () =>
                {
                    if (_mindSystem.TryGetMind(args.Target, out var mindId, out var mind))
                        _role.MindTryRemoveAllAntagRoles((mindId, mind));

                    _autolog.LogToDiscord(string.Join(": ", removeAntagName, Loc.GetString("admin-verb-remove-antag")), player.Name);
                },
                Impact = LogImpact.High,
                Message = string.Join(": ", removeAntagName, Loc.GetString("admin-verb-remove-antag")),
            };
            args.Verbs.Add(removeAntag);
        }

        // Sunset - Malfunctioning AI. Only offered for entities that actually are a station AI
        // (the brain held inside a core) - handing malf modules to a walking crewmember would
        // do nothing but confuse everyone.
        if (HasComp<StationAiHeldComponent>(args.Target))
        {
            var malfAiName = Loc.GetString("admin-verb-text-make-malf-ai");
            Verb malfAi = new()
            {
                Text = malfAiName,
                Category = VerbCategory.Antag,
                Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/Actions/malfunction.png")),
                Act = () =>
                {
                    _antag.ForceMakeAntag<MalfAiRuleComponent>(targetPlayer, DefaultMalfAiRule);
                    _autolog.LogToDiscord(string.Join(": ", malfAiName, Loc.GetString("admin-verb-make-malf-ai")), player.Name);
                },
                Impact = LogImpact.High,
                Message = string.Join(": ", malfAiName, Loc.GetString("admin-verb-make-malf-ai")),
            };
            args.Verbs.Add(malfAi);
        }

        var paradoxCloneName = Loc.GetString("admin-verb-text-make-paradox-clone");
        Verb paradox = new()
        {
            Text = paradoxCloneName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "ParadoxClone"),
            Act = () =>
            {
                var ruleEnt = _gameTicker.AddGameRule(ParadoxCloneRuleId);

                if (!TryComp<ParadoxCloneRuleComponent>(ruleEnt, out var paradoxCloneRuleComp))
                    return;

                paradoxCloneRuleComp.OriginalBody = args.Target; // override the target player

                _gameTicker.StartGameRule(ruleEnt);
                _autolog.LogToDiscord(string.Join(": ", paradoxCloneName, Loc.GetString("admin-verb-make-paradox-clone")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", paradoxCloneName, Loc.GetString("admin-verb-make-paradox-clone")),
        };

        var wizardName = Loc.GetString("admin-verb-text-make-wizard");
        Verb wizard = new()
        {
            Text = wizardName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Interface/Misc/job_icons.rsi"), "Wizard"),
            Act = () =>
            {
                // Wizard has no rule components as of writing, but I gotta put something here to satisfy the machine so just make it wizard mind rule :)
                _antag.ForceMakeAntag<WizardRoleComponent>(targetPlayer, DefaultWizardRule);
                _autolog.LogToDiscord(string.Join(": ", wizardName, Loc.GetString("admin-verb-make-wizard")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", wizardName, Loc.GetString("admin-verb-make-wizard")),
        };
        args.Verbs.Add(wizard);

        var ninjaName = Loc.GetString("admin-verb-text-make-space-ninja");
        Verb ninja = new()
        {
            Text = ninjaName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Objects/Weapons/Melee/energykatana.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<NinjaRoleComponent>(targetPlayer, DefaultNinjaRule);
                _autolog.LogToDiscord(string.Join(": ", ninjaName, Loc.GetString("admin-verb-make-space-ninja")), player.Name); //Starlight
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", ninjaName, Loc.GetString("admin-verb-make-space-ninja")),
        };
        args.Verbs.Add(ninja);

        if (HasComp<HumanoidAppearanceComponent>(args.Target)) // only humanoids can be cloned
            args.Verbs.Add(paradox);

        var changelingName = Loc.GetString("admin-verb-text-make-changeling");
        Verb changeling = new()
        {
            Text = changelingName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Goobstation/Changeling/changeling_abilities.rsi"), "armblade"),
            Act = () =>
            {
                _antag.ForceMakeAntag<Content.Server._Goobstation.Changeling.GameTicking.Rules.ChangelingRuleComponent>(targetPlayer, DefaultChangelingRule);
                _autolog.LogToDiscord(Loc.GetString("admin-verb-make-changeling"), player.Name);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-changeling"),
        };
        args.Verbs.Add(changeling);

        Verb vampire = new()
        {
            Text = Loc.GetString("admin-verb-text-make-vampire"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Starlight/Vampire/actions_vampire.rsi"), "select_class"), // Starlight
            Act = () =>
            {
                _antag.ForceMakeAntag<VampireRuleComponent>(targetPlayer, DefaultVampireRule);
                _autolog.LogToDiscord(Loc.GetString("admin-verb-make-vampire"), player.Name);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-vampire"),
        };
        args.Verbs.Add(vampire);

		var selfagentName = Loc.GetString("admin-verb-text-make-selfagent");
        Verb selfagent = new()
        {
            Text = selfagentName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Starlight/Objects/Specific/SELF/freemag.rsi"), "icon"),
            Act = () =>
            {
                _antag.ForceMakeAntag<SELFRuleComponent>(targetPlayer, DefaultSELFRule);
                _autolog.LogToDiscord(string.Join(": ", selfagentName, Loc.GetString("admin-verb-make-selfagent")), player.Name);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", selfagentName, Loc.GetString("admin-verb-make-selfagent")),
        };
        args.Verbs.Add(selfagent);

        Verb devil = new()
        {
            Text = Loc.GetString("admin-verb-text-make-devil"),
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Effects/fire.rsi"), "fire"),
            Act = () =>
            {
                _antag.ForceMakeAntag<DevilRuleComponent>(targetPlayer, DefaultDevilRule);
                _autolog.LogToDiscord(string.Join(": ", Loc.GetString("admin-verb-text-make-devil"), Loc.GetString("admin-verb-make-devil")), player.Name);
            },
            Impact = LogImpact.High,
            Message = Loc.GetString("admin-verb-make-devil")
        };
        args.Verbs.Add(devil);

        if (HasComp<ShadekinComponent>(args.Target))
        {
            Verb brighteye = new()
            {
                Text = Loc.GetString("admin-verb-text-make-brighteye"),
                Category = VerbCategory.Antag,
                Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/_Starlight/Interface/Actions/shadekin.rsi"), "rest"),
                Act = () =>
                {
                    _gameTicker.StartGameRule("TheDarkMap"); // The Dark should always be spawned for any brighteye.
                    _antag.ForceMakeAntag<BrighteyeRuleComponent>(targetPlayer, DefaultBrighteyeRule);
                    _autolog.LogToDiscord(Loc.GetString("admin-verb-make-brighteye"), player.Name);
                },
                Impact = LogImpact.High,
                Message = Loc.GetString("admin-verb-make-brighteye"),
            };
            args.Verbs.Add(brighteye);
        }

        var pirateSLName = Loc.GetString("admin-verb-text-make-pirate-sl");
        Verb pirateSL = new()
        {
            Text = pirateSLName,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new("/Textures/Objects/Misc/id_cards.rsi"), "pirate"),
            Act = () =>
            {
                _npcFactionSmite.RemoveFaction(args.Target, _smiteNanoTrasenFaction, false);
                _npcFactionSmite.AddFaction(args.Target, _smitePirateFaction);
                _outfit.SetOutfit(args.Target, PirateGearId); // Starlight
                EnsureComp<PirateAccentComponent>(args.Target); // Starlight

                if (_mindSystem.TryGetMind(args.Target, out var pirateMindId, out var pirateMind))
                {
                    _role.MindAddRole(pirateMindId, _pirateMindRole);
                    _mindSystem.TryAddObjective(pirateMindId, pirateMind, "PirateFollowCaptainObjective"); // Starlight
                }

                _antag.SendBriefing(args.Target,
                    Loc.GetString("pirate-crew-briefing"),
                    null,
                    new SoundPathSpecifier("/Audio/Ambience/Antag/pirate_start.ogg"));
                _autolog.LogToDiscord(string.Join(": ", pirateSLName, Loc.GetString("admin-verb-make-pirate-sl")), player.Name);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", pirateSLName, Loc.GetString("admin-verb-make-pirate-sl")),
        };
        args.Verbs.Add(pirateSL);
        // STARLIGHT END
    }

    /// <summary>
    /// Sunset - Hand-cast a specific The Boys team member (Butcher/Frenchie/Hughie/Mother's Milk/
    /// Kimiko, picked by defIndex into TheBoysTeam's AntagSelectionDefinition list) onto targetPlayer.
    /// Mirrors TheBoysRuleSystem.AssignTeam's own MakeAntag call, including stamping HomelanderBody
    /// first (if a Homelander already exists this round) so Butcher's kill objective still wires up
    /// via TheBoysRuleSystem.OnMemberSelected.
    /// </summary>
    private void AddTheBoysTeamVerb(GetVerbsEvent<Verb> args, ICommonSession targetPlayer, ICommonSession player, int defIndex, string textLoc, string logLoc)
    {
        var name = Loc.GetString(textLoc);
        Verb verb = new()
        {
            Text = name,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath("/Textures/Objects/Magic/magicactions.rsi"), "fireball"),
            Act = () =>
            {
                var rule = _antag.ForceGetGameRuleEnt<TheBoysTeamRuleComponent>(DefaultTheBoysTeamRule);

                if (defIndex >= rule.Comp.Definitions.Count)
                    return;

                var teamRule = Comp<TheBoysTeamRuleComponent>(rule.Owner);
                if (teamRule.HomelanderBody == null)
                {
                    var homelanderQuery = EntityQueryEnumerator<HomelanderComponent>();
                    if (homelanderQuery.MoveNext(out var homelanderUid, out _))
                        teamRule.HomelanderBody = homelanderUid;
                }

                _antag.MakeAntag(rule, targetPlayer, rule.Comp.Definitions[defIndex]);
                _autolog.LogToDiscord(string.Join(": ", name, Loc.GetString(logLoc)), player.Name);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", name, Loc.GetString(logLoc)),
        };
        args.Verbs.Add(verb);
    }

    /// <summary>
    /// Sunset - hand-cast a specific Ratvar cult role (0 = head cultist, 1 = rank-and-file cultist,
    /// indexing Definitions in Resources/Prototypes/_Sunset/Cult/roundstart.yml) onto targetPlayer.
    /// Mirrors AddTheBoysTeamVerb: grabs (or starts) the RatvarCult rule and calls MakeAntag directly
    /// with a specific definition instead of letting AntagSelection auto-fill the next open slot, so
    /// admins can deliberately pick which of the two roles to hand out.
    /// </summary>
    private void AddRatvarCultVerb(GetVerbsEvent<Verb> args, ICommonSession targetPlayer, ICommonSession player, int defIndex, string textLoc, string logLoc, string iconRsiPath)
    {
        var name = Loc.GetString(textLoc);
        Verb verb = new()
        {
            Text = name,
            Category = VerbCategory.Antag,
            Icon = new SpriteSpecifier.Rsi(new ResPath(iconRsiPath), "icon"),
            Act = () =>
            {
                var rule = _antag.ForceGetGameRuleEnt<RatvarCultRuleComponent>(DefaultRatvarCultRule);

                if (defIndex >= rule.Comp.Definitions.Count)
                    return;

                _antag.MakeAntag(rule, targetPlayer, rule.Comp.Definitions[defIndex]);
                _autolog.LogToDiscord(string.Join(": ", name, Loc.GetString(logLoc)), player.Name);
            },
            Impact = LogImpact.High,
            Message = string.Join(": ", name, Loc.GetString(logLoc)),
        };
        args.Verbs.Add(verb);
    }
}
