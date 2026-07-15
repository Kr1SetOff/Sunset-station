using System.Linq;
using Content.Server._Sunset.TheBoys.Components;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Objectives.Components;
using Content.Server.Storage.EntitySystems;
using Content.Shared._Sunset.Homelander;
using Content.Shared._Sunset.TheBoys.Components;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Storage;
using Content.Shared.Tag;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Sunset.TheBoys;

/// <summary>
/// Drives The Boys gamemode: on round start, hand-picks 6 distinct currently-playing (non-ghost)
/// crew members - preferring, but not requiring, non-Security/Command (see GetEligibleCandidates) -
/// and force-assigns them Homelander plus Butcher/Frenchie/Hughie/Mother's Milk/Kimiko, bypassing
/// the normal antag-preference pool entirely (see AssignHomelander/AssignTeam) since nobody could
/// plausibly have pre-opted into these brand new roles. Everyone keeps their existing job and body -
/// only Homelander gets replacement gear (via HomelanderGear, same as the standalone Homelander
/// rule).
///
/// Homelander and the team are two separate GameRules (TheBoysHomelanderRule/TheBoysTeamRule, see
/// game_rule.yml) run together by the TheBoys gamePreset, so admins can force/track/remove either
/// half independently - but both picks still happen in this one system, synchronously in that
/// order, so the team pick can exclude Homelander from its own pool and Butcher's "eliminate
/// Homelander" objective (see OnMemberSelected) can be wired immediately with no need to coordinate
/// across separate rule entities/events.
/// </summary>
public sealed class TheBoysRuleSystem : GameRuleSystem<TheBoysTeamRuleComponent>
{
    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly StorageSystem _storage = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    private const string HughieTag = "TheBoysHughie";
    private const string FrenchieTag = "TheBoysFrenchie";
    private const string MothersMilkTag = "TheBoysMothersMilk";

    // Security and Command already run the station's actual response to threats - handing either of
    // them Butcher's team (or Homelander himself) on top of their real job would blur that line, so
    // they're deprioritized in the candidate pool (see GetEligibleCandidates) - but still used if
    // they're the only ones available, rather than leaving a role unassigned.
    private static readonly ProtoId<DepartmentPrototype>[] ExcludedDepartments = { "Security", "Command" };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RulePlayerJobsAssignedEvent>(OnJobsAssigned);
        SubscribeLocalEvent<TheBoysTeamRuleComponent, AfterAntagEntitySelectedEvent>(OnMemberSelected);
        SubscribeLocalEvent<TheBoysHomelanderRuleComponent, AfterAntagEntitySelectedEvent>(OnHomelanderMemberSelected);
        SubscribeLocalEvent<HomelanderComponent, AttackedEvent>(OnHomelanderAttacked);
    }

    /// <summary>
    /// Retroactively links any already-picked Butcher who's still missing his kill-objective, in case
    /// he was selected (e.g. hand-cast by an admin verb) before Homelander existed this round -
    /// TryLinkButcherObjective's own Unique-objective check makes this a safe no-op for a Butcher
    /// who's already linked, so it's fine to just re-check everyone every time Homelander is (re)picked.
    /// </summary>
    private void OnHomelanderMemberSelected(Entity<TheBoysHomelanderRuleComponent> ent, ref AfterAntagEntitySelectedEvent args)
    {
        TheBoysTeamRuleComponent? teamRule = null;
        var teamQuery = EntityQueryEnumerator<TheBoysTeamRuleComponent>();
        if (teamQuery.MoveNext(out _, out var foundTeamRule))
            teamRule = foundTeamRule;

        var butcherQuery = EntityQueryEnumerator<TheBoysButcherComponent>();
        while (butcherQuery.MoveNext(out var butcherBody, out _))
            TryLinkButcherObjective(butcherBody, teamRule);
    }

    /// <summary>
    /// Butcher's crowbar (ButcherCrowbarComponent, see items.yml) deals 4x normal crowbar damage
    /// against a Homelander antagonist specifically, and normal damage against everyone else -
    /// AttackedEvent is raised directed at the victim once per hit target (unlike MeleeHitEvent,
    /// which is raised once on the weapon and shared across every target of a wide swing), so this is
    /// the one that's actually correct if the crowbar's wide swing ever also clips a bystander.
    /// </summary>
    private void OnHomelanderAttacked(Entity<HomelanderComponent> ent, ref AttackedEvent args)
    {
        if (!HasComp<ButcherCrowbarComponent>(args.Used) || !TryComp<MeleeWeaponComponent>(args.Used, out var melee))
            return;

        // +3x on top of the 1x every attack already deals = 4x total (e.g. an 8-damage crowbar hits
        // Homelander for 32).
        args.BonusDamage += melee.Damage * 3;
    }

    private void OnJobsAssigned(RulePlayerJobsAssignedEvent args)
    {
        // Homelander is picked first, deterministically, in this same synchronous method - the team
        // pick below then excludes him and uses his body directly for Butcher's kill objective.
        EntityUid? homelanderBody = null;
        var homelanderRules = 0;
        var teamRules = 0;

        var homelanderQuery = EntityQueryEnumerator<TheBoysHomelanderRuleComponent, AntagSelectionComponent>();
        while (homelanderQuery.MoveNext(out var uid, out _, out var antagSelection))
        {
            homelanderRules++;
            homelanderBody ??= AssignHomelander((uid, antagSelection), args.Players);
        }

        var teamQuery = EntityQueryEnumerator<TheBoysTeamRuleComponent, AntagSelectionComponent>();
        while (teamQuery.MoveNext(out var uid, out var teamRule, out var antagSelection))
        {
            teamRules++;
            AssignTeam((uid, antagSelection), teamRule, homelanderBody, args.Players);
        }

        // Diagnostic: if either count is 0, that half of the split rule never made it into this
        // round at all (e.g. cancelled at RoundStartAttemptEvent) - AssignHomelander/AssignTeam
        // wouldn't even run in that case, which looks identical to "picked nobody" from the outside.
        Log.Info($"TheBoys: found {homelanderRules} TheBoysHomelanderRule and {teamRules} TheBoysTeamRule entities this round ({args.Players.Length} players total).");
    }

    /// <summary>
    /// Returns eligible (humanoid, non-ghost) candidates, shuffled and with Security/Command pushed
    /// to the back rather than dropped outright - with a small crew (e.g. a 2-player test round)
    /// hard-excluding them could leave zero candidates for the team even though someone is actually
    /// available, so it's a soft preference: everyone else gets picked first, but Sec/Command still
    /// get used rather than leaving a role unassigned when they're all that's left.
    /// </summary>
    private List<ICommonSession> GetEligibleCandidates(ICommonSession[] players)
    {
        var eligible = players
            .Where(session => session.AttachedEntity is { } ent
                && HasComp<HumanoidAppearanceComponent>(ent)
                && !HasComp<GhostComponent>(ent))
            .ToList();

        _random.Shuffle(eligible);

        return eligible.OrderBy(session => IsExcludedDepartment(session.AttachedEntity!.Value)).ToList();
    }

    /// <summary>
    /// Hand-picks a single eligible candidate for Homelander's sole AntagSelectionDefinition and
    /// calls MakeAntag directly - not TryMakeAntag, which would reject every candidate via its
    /// antag-preference/profile checks since this role has a deliberately empty PrefRoles list.
    /// Returns the picked body, or null if no eligible candidate was found.
    /// </summary>
    private EntityUid? AssignHomelander(Entity<AntagSelectionComponent> rule, ICommonSession[] players)
    {
        var candidates = GetEligibleCandidates(players);
        if (candidates.Count == 0)
        {
            Log.Warning("TheBoysHomelander rule found no eligible players - Homelander will go unassigned.");
            return null;
        }

        var candidate = candidates[0];
        var def = rule.Comp.Definitions[0];

        try
        {
            _antag.MakeAntag(rule, candidate, def);
        }
        catch (Exception e)
        {
            Log.Error($"TheBoysHomelander assignment threw for {candidate}: {e}");
            return null;
        }

        return candidate.AttachedEntity;
    }

    /// <summary>
    /// Hand-picks one distinct session per AntagSelectionDefinition on this rule (Butcher, Frenchie,
    /// Hughie, Mother's Milk, Kimiko) and calls MakeAntag directly - see AssignHomelander's doc
    /// comment for why not TryMakeAntag.
    /// </summary>
    private void AssignTeam(Entity<AntagSelectionComponent> rule, TheBoysTeamRuleComponent teamRule, EntityUid? homelanderBody, ICommonSession[] players)
    {
        teamRule.HomelanderBody = homelanderBody;

        var candidates = GetEligibleCandidates(players)
            .Where(session => session.AttachedEntity != homelanderBody)
            .ToList();

        var needed = rule.Comp.Definitions.Count;
        if (candidates.Count < needed)
        {
            Log.Warning($"TheBoysTeam rule wants {needed} eligible players but only found {candidates.Count} - some roles will go unfilled.");
        }

        var index = 0;
        foreach (var def in rule.Comp.Definitions)
        {
            if (index >= candidates.Count)
                break;

            var candidate = candidates[index];
            index++;

            // Each definition is independent (Butcher, Frenchie, Hughie, Mother's Milk, Kimiko) - a
            // throw anywhere in MakeAntag's synchronous chain (including the
            // AfterAntagEntitySelectedEvent handlers it raises, e.g. OnMemberSelected below) must
            // never be allowed to abort this loop and leave the remaining team members un-assigned.
            try
            {
                _antag.MakeAntag(rule, candidate, def);
            }
            catch (Exception e)
            {
                Log.Error($"TheBoysTeam assignment threw for {candidate} (definition #{index - 1}): {e}");
            }
        }
    }

    /// <summary>
    /// Spawns protoId and tries to slip it into the body's backpack; falls back to their hands (and
    /// from there, if those are full too, it's just left sitting under them) if there's no bag or no
    /// room in it.
    /// </summary>
    private void GiveItem(EntityUid body, string protoId)
    {
        var item = Spawn(protoId, Transform(body).Coordinates);

        if (_inventory.TryGetSlotEntity(body, "back", out var backUid) &&
            TryComp<StorageComponent>(backUid, out var storage) &&
            _storage.Insert(backUid.Value, item, out _, storageComp: storage, playSound: false))
            return;

        _hands.TryPickupAnyHand(body, item, checkActionBlocker: false);
    }

    /// <summary>
    /// Wires up Butcher's real "eliminate Homelander" objective. Resolves the current Homelander live
    /// via component query instead of relying solely on TheBoysTeamRuleComponent.HomelanderBody, which
    /// used to be the only source of truth - if it was never populated (e.g. an admin hand-casting
    /// Butcher via AdminVerbSystem's "Make TheBoys Butcher" verb before Homelander existed yet this
    /// round), Butcher silently ended up with no objective at all, forever. This is safe and correct
    /// to call repeatedly and from either direction (OnMemberSelected for Butcher himself, and
    /// OnHomelanderMemberSelected retroactively once Homelander lands) - TryAddObjective's own Unique
    /// check makes re-linking an already-linked Butcher a safe no-op.
    /// </summary>
    /// <returns>False only when there's no Homelander picked yet this round to link to.</returns>
    private bool TryLinkButcherObjective(EntityUid butcherBody, TheBoysTeamRuleComponent? teamRule = null)
    {
        var homelanderQuery = EntityQueryEnumerator<HomelanderComponent>();
        if (!homelanderQuery.MoveNext(out var homelanderBody, out _))
            return false;

        if (teamRule != null)
            teamRule.HomelanderBody = homelanderBody;

        if (!_mind.TryGetMind(homelanderBody, out var homelanderMind, out _) ||
            !_mind.TryGetMind(butcherBody, out var butcherMind, out var butcherMindComp))
            return false;

        EnsureComp<TargetOverrideComponent>(butcherBody).Target = homelanderMind;
        _mind.TryAddObjective(butcherMind, butcherMindComp, "TheBoysKillHomelanderObjective");
        return true;
    }

    private bool IsExcludedDepartment(EntityUid body)
    {
        if (!_mind.TryGetMind(body, out var mindId, out _))
            return false;

        if (!_jobs.MindTryGetJobId(mindId, out var jobId) || jobId is null)
            return false;

        foreach (var deptId in ExcludedDepartments)
        {
            if (_prototype.TryIndex(deptId, out var dept) && dept.Roles.Contains(jobId.Value))
                return true;
        }

        return false;
    }

    private void OnMemberSelected(Entity<TheBoysTeamRuleComponent> rule, ref AfterAntagEntitySelectedEvent args)
    {
        var body = args.EntityUid;

        try
        {
            if (HasComp<TheBoysButcherComponent>(body))
            {
                _antag.SendBriefing(body, Loc.GetString("theboys-role-greeting-butcher"), null, null);

                if (!TryLinkButcherObjective(body, rule.Comp))
                    Log.Warning($"TheBoys: could not wire up {ToPrettyString(body)}'s kill-Homelander objective yet (no Homelander picked this round so far) - will retry once Homelander is selected.");

                GiveItem(body, "TheBoysButcherCrowbar");
                GiveItem(body, "TheBoysWeaponsPermit");
                GiveItem(body, "EncryptionKeyTheBoys");

                return;
            }

            if (HasComp<TheBoysTeammateComponent>(body))
            {
                _antag.SendBriefing(body, Loc.GetString("theboys-role-greeting-teammate"), null, null);

                if (_mind.TryGetMind(body, out var teammateMind, out var teammateMindComp))
                    _mind.TryAddObjective(teammateMind, teammateMindComp, "TheBoysHelpButcherObjective");

                GiveItem(body, "EncryptionKeyTheBoys");

                if (_tag.HasTag(body, HughieTag))
                    GiveItem(body, "TheBoysHughieCoolBroPaper");

                if (_tag.HasTag(body, FrenchieTag))
                    GiveItem(body, "TheBoysWeaponsPermit");

                if (_tag.HasTag(body, MothersMilkTag))
                {
                    GiveItem(body, "TheBoysMothersMilkShotgun");
                    GiveItem(body, "TheBoysWeaponsPermit");
                }
            }
        }
        catch (Exception e)
        {
            Log.Error($"TheBoys member setup threw for {ToPrettyString(body)}: {e}");
        }
    }
}
