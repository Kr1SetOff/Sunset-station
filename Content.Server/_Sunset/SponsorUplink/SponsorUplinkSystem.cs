// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Storage.EntitySystems;
using Content.Shared._Sunset.Biocode;
using Content.Shared._Sunset.SponsorTier;
using Content.Shared.GameTicking;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Prototypes;

namespace Content.Server._Sunset.SponsorUplink;

/// <summary>
/// Auto-grants a Sponsor Uplink to linked Boosty sponsors (tier 2+) at every spawn - fires on
/// <see cref="PlayerSpawnCompleteEvent"/>, which runs for every player regardless of job, after
/// normal starting-gear/loadout equip. The uplink's per-round coin balance is fixed by which tier
/// prototype gets spawned (see Resources/Prototypes/_Sunset/SponsorUplink/entities.yml); it does
/// not persist between rounds, matching how traitor/antag uplinks work.
/// </summary>
public sealed class SponsorUplinkSystem : EntitySystem
{
    [Dependency] private readonly ISunsetSponsorTierReader _sponsorTier = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly StorageSystem _storage = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;

    /// <summary>
    /// Sponsor tier -> uplink prototype to spawn. Tiers below 2 (None, Zombie) get nothing.
    /// </summary>
    private static readonly Dictionary<int, string> TierUplinkPrototypes = new()
    {
        [2] = "SponsorUplinkTier2",
        [3] = "SponsorUplinkTier3",
        [4] = "SponsorUplinkTier4",
        [5] = "SponsorUplinkTier5",
    };

    /// <summary>
    /// The general tabs every sponsor sees, regardless of job.
    /// </summary>
    private static readonly ProtoId<StoreCategoryPrototype>[] GeneralCategories =
    {
        "SponsorUplinkUniforms",
        "SponsorUplinkOuterwear",
        "SponsorUplinkHeadwear",
        "SponsorUplinkAccessories",
        "SponsorUplinkMisc",
    };

    /// <summary>
    /// Job id -> the one department tab that job's uplink additionally unlocks (on top of the
    /// general tabs). Jobs not listed here (e.g. Borg/StationAi, which don't use the humanoid
    /// clothing loadout system at all) only ever see the general tabs.
    /// </summary>
    private static readonly Dictionary<string, ProtoId<StoreCategoryPrototype>> JobDepartmentCategory = new()
    {
        // Civilian
        ["JobClown"] = "SponsorUplinkDeptCivilian",
        ["JobAssistant"] = "SponsorUplinkDeptCivilian",
        ["JobLawyer"] = "SponsorUplinkDeptCivilian",
        ["JobMime"] = "SponsorUplinkDeptCivilian",
        ["JobBartender"] = "SponsorUplinkDeptCivilian",
        ["JobChef"] = "SponsorUplinkDeptCivilian",
        ["JobJanitor"] = "SponsorUplinkDeptCivilian",
        ["JobServiceWorker"] = "SponsorUplinkDeptCivilian",
        ["JobLibrarian"] = "SponsorUplinkDeptCivilian",
        ["JobMusician"] = "SponsorUplinkDeptCivilian",
        ["JobReporter"] = "SponsorUplinkDeptCivilian",
        ["JobPerformer"] = "SponsorUplinkDeptCivilian",
        ["JobBoxer"] = "SponsorUplinkDeptCivilian",
        ["JobZookeeper"] = "SponsorUplinkDeptCivilian",
        ["JobBotanist"] = "SponsorUplinkDeptCivilian",
        ["JobChaplain"] = "SponsorUplinkDeptCivilian",
        // Security
        ["JobHeadOfSecurity"] = "SponsorUplinkDeptSecurity",
        ["JobWarden"] = "SponsorUplinkDeptSecurity",
        ["JobSecurityOfficer"] = "SponsorUplinkDeptSecurity",
        ["JobDetective"] = "SponsorUplinkDeptSecurity",
        ["JobSecurityCadet"] = "SponsorUplinkDeptSecurity",
        ["JobBrigmedic"] = "SponsorUplinkDeptSecurity",
        ["JobDutyOfficer"] = "SponsorUplinkDeptSecurity",
        // Cargo
        ["JobSalvageSpecialist"] = "SponsorUplinkDeptCargo",
        ["JobMiningSpecialist"] = "SponsorUplinkDeptCargo",
        ["JobSalvageLead"] = "SponsorUplinkDeptCargo",
        ["JobQuartermaster"] = "SponsorUplinkDeptCargo",
        ["JobCargoTechnician"] = "SponsorUplinkDeptCargo",
        ["JobMailTech"] = "SponsorUplinkDeptCargo",
        ["JobBitrunner"] = "SponsorUplinkDeptCargo",
        // Command
        ["JobCaptain"] = "SponsorUplinkDeptCommand",
        ["JobHeadOfPersonnel"] = "SponsorUplinkDeptCommand",
        // Representatives
        ["JobNanoTrasenRepresentative"] = "SponsorUplinkDeptRepresentatives",
        ["JobBlueShield"] = "SponsorUplinkDeptRepresentatives",
        ["JobMagistrate"] = "SponsorUplinkDeptRepresentatives",
        ["JobIAA"] = "SponsorUplinkDeptRepresentatives",
        ["JobNanotrasenCareerTrainer"] = "SponsorUplinkDeptRepresentatives",
        // Engineering
        ["JobChiefEngineer"] = "SponsorUplinkDeptEngineering",
        ["JobTechnicalAssistant"] = "SponsorUplinkDeptEngineering",
        ["JobStationEngineer"] = "SponsorUplinkDeptEngineering",
        ["JobAtmosphericTechnician"] = "SponsorUplinkDeptEngineering",
        // Medical
        ["JobChiefMedicalOfficer"] = "SponsorUplinkDeptMedical",
        ["JobChemist"] = "SponsorUplinkDeptMedical",
        ["JobMedicalDoctor"] = "SponsorUplinkDeptMedical",
        ["JobMedicalIntern"] = "SponsorUplinkDeptMedical",
        ["JobPsychologist"] = "SponsorUplinkDeptMedical",
        ["JobParamedic"] = "SponsorUplinkDeptMedical",
        ["JobGeneticist"] = "SponsorUplinkDeptMedical",
        ["JobSurgeon"] = "SponsorUplinkDeptMedical",
        // Science
        ["JobResearchDirector"] = "SponsorUplinkDeptScience",
        ["JobScientist"] = "SponsorUplinkDeptScience",
        ["JobResearchAssistant"] = "SponsorUplinkDeptScience",
        ["JobRoboticist"] = "SponsorUplinkDeptScience",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
    }

    private void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        var tier = _sponsorTier.GetSponsorTier(args.Player);
        if (!TierUplinkPrototypes.TryGetValue(tier, out var uplinkProto))
            return;

        var uplink = Spawn(uplinkProto, Transform(args.Mob).Coordinates);

        // Pre-bind the biocode to the spawning body, rather than requiring the owner to use the
        // "Install biocode" verb - this is an automatic perk, so it should already be locked to
        // them before they even open their backpack.
        var biocode = EnsureComp<BiocodeComponent>(uplink);
        biocode.OwnerEntity = args.Mob;
        biocode.OwnerName = Name(args.Mob);

        // Restrict this uplink instance's tabs to the general ones plus (if applicable) the
        // buyer's own department, so e.g. a bartender never sees the security-only listings.
        if (TryComp<StoreComponent>(uplink, out var store))
        {
            store.Categories = new HashSet<ProtoId<StoreCategoryPrototype>>(GeneralCategories);
            if (args.JobId != null && JobDepartmentCategory.TryGetValue(args.JobId, out var deptCategory))
                store.Categories.Add(deptCategory);
        }

        InsertIntoInventory(args.Mob, uplink);
    }

    private void InsertIntoInventory(EntityUid mob, EntityUid item)
    {
        if (_inventory.TryGetSlotEntity(mob, "back", out var backUid) &&
            TryComp<StorageComponent>(backUid, out var storage) &&
            _storage.Insert(backUid.Value, item, out _, storageComp: storage, playSound: false))
            return;

        _hands.TryPickupAnyHand(mob, item, checkActionBlocker: false);
    }
}
