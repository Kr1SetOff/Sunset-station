using System.Linq;
using Content.Server._Sunset.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking;
using Content.Shared._Sunset.CCVar;
using Content.Shared._Sunset.SponsorTier;
using Content.Shared.Database;
using Content.Shared.GameTicking;
using Content.Shared.Players;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Random;

namespace Content.Server.Antag;

// 🌇Sunset🌇 - Boosty sponsor tier 5's "99% guaranteed antagonist" perk. Kept in its own partial file
// (matching this repo's convention of splitting manager/system classes by feature, e.g. NullLinkPlayerManager)
// since it's a bolt-on perk, not core antag-selection logic.
public sealed partial class AntagSelectionSystem
{
    [Dependency] private ISunsetSponsorTierReader _sunsetSponsorTiers = default!;
    [Dependency] private IConfigurationManager _sunsetCfg = default!;

    private int _sunsetTier5ForcedThisRound;

    /// <summary>
    /// Presets where "agents" (Syndicate-style traitor antagonists) actually exist - the only
    /// presets the Spy sponsor guarantee (and the tier-5 sweep) are allowed to hand Spy out in.
    /// Mirrors the preset list that includes SunSetTraitorExtrasRule (see
    /// Resources/Prototypes/game_presets.yml and Resources/Prototypes/_Starlight/game_presets.yml) -
    /// keep in sync if that list changes.
    /// </summary>
    private static readonly HashSet<string> SunsetSpyAgentPresets = new()
    {
        "Traitor",
        "Traitorling",
        "VampTraitor",
    };

    private const int SunsetSpyMinPlayers = 5;

    /// <summary>
    /// Whether Spy can be handed out (via either sponsor bypass below) this round at all - requires
    /// both a preset that actually has agents (not Extended/Greenshift/etc) and enough players
    /// connected that a spy round makes sense.
    /// </summary>
    private bool IsSunsetSpyAllowedThisRound(int playerCount)
    {
        if (playerCount < SunsetSpyMinPlayers)
            return false;

        var presetId = GameTicker.CurrentPreset?.ID;
        return presetId != null && SunsetSpyAgentPresets.Contains(presetId);
    }

    private void InitializeSunset()
    {
        // NOTE: does NOT subscribe to RulePlayerJobsAssignedEvent here - AntagSelectionSystem already
        // subscribes once in its main Initialize(), and RobustToolbox disallows the same subscriber
        // registering twice for the same event type. OnSunsetJobsAssigned is instead called directly
        // from the end of OnJobsAssigned (see AntagSelectionSystem.cs).
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnSunsetRoundRestart);
    }

    private void OnSunsetRoundRestart(RoundRestartCleanupEvent args)
    {
        _sunsetTier5ForcedThisRound = 0;
    }

    private void OnSunsetJobsAssigned(RulePlayerJobsAssignedEvent args)
    {
        // 🌇Sunset🌇 - named sponsor role guarantee for Spy. Homelander is intentionally NOT offered
        // here (and is excluded from the generic tier-5 sweep below, see TryForceSunsetTier5Antag) -
        // it must never be handed out automatically, only via an admin explicitly force-making someone
        // Homelander (see AdminVerbSystem.Antags.cs) or manually starting the Homelander GameRule.
        // Spy starts at tier 2 (Syndicate Agent) - tier 1 (Zombie) does not get the Spy guarantee.
        // Spy is further gated to agent-bearing presets with >= 5 players (see IsSunsetSpyAllowedThisRound) -
        // without this, the bypass below would happily force Spy into Extended/Greenshift or a
        // near-empty server, since StartGameRule/AddGameRule don't consult GameRuleComponent.MinPlayers.
        var spyAllowed = IsSunsetSpyAllowedThisRound(args.Players.Length);

        if (spyAllowed)
        {
            foreach (var session in args.Players)
            {
                if (_sunsetSponsorTiers.GetSponsorTier(session) < 2)
                    continue;

                if (_role.MindIsAntagonist(session.GetMind()))
                    continue; // already an antagonist (e.g. just got Homelander above) - nothing to do.

                TryForceSunsetNamedAntag(session, "Spy");
            }
        }

        var maxPerRound = _sunsetCfg.GetCVar(SunsetCCVars.SunsetTier5MaxForcedAntagsPerRound);

        foreach (var session in args.Players)
        {
            if (_sunsetTier5ForcedThisRound >= maxPerRound)
                break;

            if (_sunsetSponsorTiers.GetSponsorTier(session) < 5)
                continue;

            if (_role.MindIsAntagonist(session.GetMind()))
                continue; // already an antagonist through normal selection - no need to force anything.

            if (!RobustRandom.Prob(0.99f))
                continue;

            if (TryForceSunsetTier5Antag(session, spyAllowed))
                _sunsetTier5ForcedThisRound++;
        }
    }

    /// <summary>
    /// Tries to force-assign a sponsor into a specific named antag GameRule (e.g. "Spy", "Homelander"),
    /// starting that rule first if it isn't already active this round. Bypasses the normal preference/pool
    /// lottery the same way <see cref="TryForceSunsetTier5Antag"/> does, but targets one rule by prototype
    /// ID instead of picking from every currently-active rule.
    /// </summary>
    private bool TryForceSunsetNamedAntag(ICommonSession session, string ruleId)
    {
        var ruleEntity = EntityUid.Invalid;

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out _, out _))
        {
            if (MetaData(uid).EntityPrototype?.ID != ruleId)
                continue;

            ruleEntity = uid;
            break;
        }

        if (ruleEntity == EntityUid.Invalid && !GameTicker.StartGameRule(ruleId, out ruleEntity))
            return false;

        if (!TryComp<AntagSelectionComponent>(ruleEntity, out var antag))
            return false;

        foreach (var def in antag.Definitions)
        {
            if (!def.PickPlayer)
                continue;

            if (!TryMakeAntag((ruleEntity, antag), session, def, checkPref: false))
                continue;

            _adminLogger.Add(LogType.AntagSelection,
                $"Sunset sponsor guarantee force-assigned {session} as antagonist: {ToPrettyString(ruleEntity)}");
            return true;
        }

        return false;
    }

    /// <summary>
    /// Tries to force-assign a tier-5 sponsor into any currently-active, player-pickable antag definition,
    /// bypassing the normal preference/pool lottery (but not the underlying eligibility checks in
    /// <see cref="TryMakeAntag"/>, e.g. whitelist/blacklist/already-antag exclusions still apply).
    /// Homelander is excluded even if its GameRule happens to be active (e.g. an admin started it) -
    /// that role must only ever be handed out by explicit admin action, never by this generic sweep.
    /// Spy is excluded unless <paramref name="spyAllowed"/> is true (agent-bearing preset + enough
    /// players - see IsSunsetSpyAllowedThisRound), for the same reason - it could otherwise still be
    /// active from an admin-triggered start on the wrong preset.
    /// </summary>
    private bool TryForceSunsetTier5Antag(ICommonSession session, bool spyAllowed)
    {
        var candidates = new List<(EntityUid Rule, AntagSelectionComponent Comp, AntagSelectionDefinition Def)>();

        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var antag, out _))
        {
            var protoId = MetaData(uid).EntityPrototype?.ID;

            if (protoId == "Homelander")
                continue;

            // TheBoys hand-picks its own fixed 6-person cast directly (see TheBoysRuleSystem) -
            // sweeping a tier-5 sponsor into one of its definitions here would either double-assign
            // an already-picked slot or hijack a curated narrative role for an unrelated player.
            // Two separate GameRules (see Resources/Prototypes/_Sunset/TheBoys/game_rule.yml) run
            // together for this mode, so both IDs need excluding.
            if (protoId is "TheBoysHomelander" or "TheBoysTeam")
                continue;

            if (protoId == "Spy" && !spyAllowed)
                continue;

            foreach (var def in antag.Definitions)
            {
                if (def.PickPlayer)
                    candidates.Add((uid, antag, def));
            }
        }

        RobustRandom.Shuffle(candidates);

        foreach (var (uid, antag, def) in candidates)
        {
            if (!TryMakeAntag((uid, antag), session, def, checkPref: false))
                continue;

            _adminLogger.Add(LogType.AntagSelection,
                $"Sunset tier-5 guarantee force-assigned {session} as antagonist: {ToPrettyString(uid)}");
            return true;
        }

        return false;
    }
}
