using Content.Shared._Starlight.Achievement;
using Content.Shared.Atmos.Components;
using Content.Shared.Examine;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory.Events;
using Content.Shared.Medical;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Slippery;
using Content.Shared.Xenoarchaeology.Artifact.Components;
using Content.Shared.Xenoarchaeology.Artifact;
using Robust.Shared.Player;

namespace Content.Server._Starlight.Achievement;

// 🌇Sunset🌇 - achievements ported from sunset-station's _Sunset/Achievements/achievements.yml.
// That fork's achievement prototype is vanilla SS14's (target/triggers/reward/jobWhitelist), which
// this repo's AchievementPrototype (Content.Shared._Starlight.Achievement) replaced with a
// progressType/requirements one - the two schemas aren't wire-compatible, so instead of copying the
// YAML we re-implement each trigger against the existing progress-tracking API here and reference the
// resulting keys from Resources/Prototypes/_Starlight/Achievement/achievements.yml.
//
// Not every source achievement made the cut: a handful (document_signed, bible_banish_devil,
// heretic_ascension, dragon/changeling-devour chain, foreign_limb_attached, strange_pill_fed,
// self_cuffed/dead_player_cuffed, wanted_without_kills, puddle_cleaned, devil_contract) need either a
// new event on an unrelated core system or content (Heretic, MobTrevor) that doesn't exist in this
// fork, and were left out to keep this port to hooks on events that already fire.
public sealed partial class AchievementSystem
{
    [Dependency] private SharedHandsSystem _handsSunset = default!;

    private const string FoldingChairIdMarker = "ChairFolding";
    private const string HotStuffJobId = "AtmosphericTechnician";
    private const string BestSecurityOfficerJobId = "SecurityOfficer";
    private const string CircusCaptainJobId = "Clown";
    private const string MiscalculatedJobId = "Scientist";
    private const string SingularityArtifactNodeId = "XenoArtifactSingularity";
    private const float HotStuffBurnSeconds = 60f;
    private const float SpaceWithoutSuitTickSeconds = 1f;

    private readonly Dictionary<EntityUid, float> _sunsetFireTimeTracking = new();
    private readonly Dictionary<EntityUid, float> _sunsetSpaceTimeTracking = new();

    private void InitializeSunset()
    {
        SubscribeLocalEvent<SlipperyComponent, SlipEvent>(OnSlipSunset);
        SubscribeLocalEvent<MobStateComponent, TargetDefibrillatedEvent>(OnTargetDefibrillatedSunset);
        SubscribeLocalEvent<ExaminedEvent>(OnExaminedSunset);
        SubscribeLocalEvent<DidEquipEvent>(OnDidEquipSunset);
        SubscribeLocalEvent<XenoArtifactNodeComponent, XenoArtifactNodeActivatedEvent>(OnArtifactNodeActivatedSunset);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        UpdateSunsetTimedTriggers(frameTime);
    }

    // WhatStation: 15 accumulated seconds in space without a suit, per round.
    // HotStuff: burn continuously for 60s as an Atmospheric Technician.
    private void UpdateSunsetTimedTriggers(float frameTime)
    {
        var query = EntityQueryEnumerator<ActorComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var actor, out var xform))
        {
            if (xform.GridUid == null && !IsWearingSuitSunset(uid))
            {
                var accumulated = _sunsetSpaceTimeTracking.GetValueOrDefault(uid) + frameTime;
                if (accumulated >= SpaceWithoutSuitTickSeconds)
                {
                    AddProgressAndCheck(actor.PlayerSession, AchievementProgressKeys.SpaceWithoutSuitSeconds);
                    accumulated -= SpaceWithoutSuitTickSeconds;
                }

                _sunsetSpaceTimeTracking[uid] = accumulated;
            }
            else
            {
                _sunsetSpaceTimeTracking.Remove(uid);
            }

            if (TryComp<FlammableComponent>(uid, out var flammable) && flammable.OnFire)
            {
                var burned = _sunsetFireTimeTracking.GetValueOrDefault(uid) + frameTime;
                if (burned >= HotStuffBurnSeconds)
                {
                    if (TryGetJobId(uid, out var jobId) && jobId == HotStuffJobId)
                        QueueUnlockAchievement(actor.PlayerSession, "sunset_hot_stuff");

                    _sunsetFireTimeTracking.Remove(uid);
                }
                else
                {
                    _sunsetFireTimeTracking[uid] = burned;
                }
            }
            else
            {
                _sunsetFireTimeTracking.Remove(uid);
            }
        }
    }

    private bool IsWearingSuitSunset(EntityUid uid)
    {
        if (!_inventory.TryGetSlotEntity(uid, "outerClothing", out var suit))
            return false;

        var protoId = MetaData(suit.Value).EntityPrototype?.ID ?? string.Empty;
        return protoId.Contains("Hardsuit") || protoId.Contains("SpaceSuit") || protoId.Contains("EVA");
    }

    // PGSher: die 10 times. Revenge: Hamlet (the ghost-role hamster) kills anyone.
    private void OnMobStateChangedSunset(MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || args.OldMobState == MobState.Dead)
            return;

        if (TryComp<ActorComponent>(args.Target, out var actor))
            AddProgressAndCheck(actor.PlayerSession, AchievementProgressKeys.DeathCount);

        if (args.Origin is { } origin
            && IsHamletSunset(origin)
            && origin != args.Target
            && TryComp<ActorComponent>(origin, out var hamletActor))
        {
            QueueUnlockAchievement(hamletActor.PlayerSession, "sunset_revenge");
        }
    }

    private bool IsHamletSunset(EntityUid uid)
    {
        if (!TryComp<MetaDataComponent>(uid, out var meta))
            return false;

        if (meta.EntityPrototype?.ID == "MobHamsterHamlet")
            return true;

        return meta.EntityName.Contains("Гамлет") || meta.EntityName.Contains("Hamlet");
    }

    // CarpoLover: kill the Shiva spider boss. DangerousFurniture: kill anyone while holding a folding chair.
    private void OnKillReportedSunset(ICommonSession killerSession, EntityUid killerUid, EntityUid victim)
    {
        if (TryComp<MetaDataComponent>(victim, out var victimMeta)
            && victimMeta.EntityPrototype?.ID == "MobSpiderShiva")
        {
            QueueUnlockAchievement(killerSession, "sunset_carpo_lover");
        }

        if (_handsSunset.TryGetActiveItem(killerUid, out var held)
            && TryComp<MetaDataComponent>(held, out var heldMeta)
            && (heldMeta.EntityPrototype?.ID.Contains(FoldingChairIdMarker) ?? false))
        {
            QueueUnlockAchievement(killerSession, "sunset_dangerous_furniture");
        }
    }

    // God: revive 12 people, per round.
    private void OnTargetDefibrillatedSunset(EntityUid target, MobStateComponent component, ref TargetDefibrillatedEvent ev)
    {
        if (!TryComp<ActorComponent>(ev.User, out var actor))
            return;

        if (component.CurrentState is MobState.Alive or MobState.Critical)
            AddProgressAndCheck(actor.PlayerSession, AchievementProgressKeys.ReviveCount);
    }

    // UnfunnyClown: slip 228 times. BestSecurityOfficer: slip 100 times as a Security Officer, per round.
    private void OnSlipSunset(EntityUid uid, SlipperyComponent component, ref SlipEvent ev)
    {
        if (!TryComp<ActorComponent>(ev.Slipped, out var actor))
            return;

        AddProgressAndCheck(actor.PlayerSession, AchievementProgressKeys.SlipCount);

        if (TryGetJobId(ev.Slipped, out var jobId) && jobId == BestSecurityOfficerJobId)
            AddProgressAndCheck(actor.PlayerSession, AchievementProgressKeys.SlipJob(BestSecurityOfficerJobId));
    }

    // Curious: examine 100 things, per round.
    private void OnExaminedSunset(ExaminedEvent ev)
    {
        if (TryComp<ActorComponent>(ev.Examiner, out var actor))
            AddProgressAndCheck(actor.PlayerSession, AchievementProgressKeys.ExamineCount);
    }

    // CircusCaptain: the clown puts on the captain's hat.
    private void OnDidEquipSunset(DidEquipEvent args)
    {
        var protoId = MetaData(args.Equipment).EntityPrototype?.ID ?? string.Empty;
        if (!protoId.Contains("Captain") || !protoId.Contains("Hat"))
            return;

        if (TryGetJobId(args.Equipee, out var jobId) && jobId == CircusCaptainJobId)
            QueueUnlockAchievement(args.Equipee, "sunset_circus_captain");
    }

    // Miscalculated: a Scientist activates an artifact node that collapses into a singularity.
    private void OnArtifactNodeActivatedSunset(EntityUid uid, XenoArtifactNodeComponent component, ref XenoArtifactNodeActivatedEvent args)
    {
        if (args.User is not { } user || !TryComp<MetaDataComponent>(uid, out var meta))
            return;

        if (meta.EntityPrototype?.ID != SingularityArtifactNodeId)
            return;

        if (TryGetJobId(user, out var jobId) && jobId == MiscalculatedJobId)
            QueueUnlockAchievement(user, "sunset_miscalculated");
    }

    // Survivor: end the round alive. RockBottom: end the round in critical condition.
    // Professional: an antagonist completes every one of their objectives.
    private void OnRoundEndTextSunset()
    {
        foreach (var session in _playerManager.Sessions)
        {
            if (session.AttachedEntity is not { } entity || !TryComp<MobStateComponent>(entity, out var mobState))
                continue;

            if (mobState.CurrentState == MobState.Alive)
                QueueUnlockAchievement(session, "sunset_survivor");
            else if (mobState.CurrentState == MobState.Critical)
                QueueUnlockAchievement(session, "sunset_rock_bottom");

            if (_mind.TryGetMind(session.UserId, out Entity<MindComponent>? mindEnt)
                && mindEnt is { } mind
                && DidMindGreentext(mind))
            {
                QueueUnlockAchievement(session, "sunset_professional");
            }
        }
    }
}
