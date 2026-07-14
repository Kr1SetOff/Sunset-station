// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using System.Threading.Tasks;
using Content.Server._Sunset.Arena.Components;
using Content.Server.Chat.Managers;
using Content.Server.Database;
using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.KillTracking;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Shared._Sunset.Arena;
using Content.Shared._Sunset.MartialArts;
using Content.Shared._Sunset.MartialArts.Systems;
using Content.Shared.Chat;
using Content.Shared.Gibbing;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.ContentPack;
using Robust.Shared.EntitySerialization;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._Sunset.Arena;

/// <summary>
/// Ghost combat arena. A ghost creates a match in one of three modes, other ghosts queue up,
/// and after a short timer everyone is dropped into a temporary body on a generated arena map to
/// fight until one remains. The survivor is gibbed, the map is cleaned up, and after a cooldown a
/// new match can be created.
///
/// Players keep their existing mind: it is moved into a temporary body and, on death, the player is
/// returned to a fresh observer ghost. Because no extra minds are created, the temporary bodies do
/// not show up in the end-of-round player list.
/// </summary>
public sealed class ArenaSystem : EntitySystem
{
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly MapLoaderSystem _mapLoader = default!;
    [Dependency] private readonly ITileDefinitionManager _tileDef = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IResourceManager _res = default!;
    [Dependency] private readonly ISharedPlayerManager _players = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly GhostSystem _ghost = default!;
    [Dependency] private readonly GibbingSystem _gibbing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly SharedMartialArtsSystem _martialArts = default!;

    private const float QueueSeconds = 20f;
    private const float CooldownSeconds = 5f;
    private const int MaxFighters = 12;
    private const int ArenaSize = 17;
    private const int LeaderboardSize = 10;

    // Below this many connected players, EnsureArena loads SmallMapPath; at or above it, LargeMapPath.
    // Falls back to the single OptionalMapPath (then procedural generation) if the picked file is missing.
    private const int SmallPopulationThreshold = 10;
    private const string SmallMapPath = "Maps/_Sunset/Arena/arena_small.yml";
    private const string LargeMapPath = "Maps/_Sunset/Arena/arena_large.yml";
    private const string OptionalMapPath = "Maps/arena.yml";
    private const string FighterGear = "ArenaGear";
    private const string FloorTile = "FloorSteel";
    private static readonly SoundSpecifier GatherSound = new SoundPathSpecifier("/Audio/_Starlight/Misc/ghost_ping.ogg");

    private static readonly EntProtoId FighterProto = "MobHuman";
    private static readonly EntProtoId WallProto = "WallSolid";
    private static readonly EntProtoId SpectatorGhostProto = "MobObserver";

    // Curated kits, not a flat random pool - every fighter in a match gets ALL the items (and the
    // martial art, if any) in one randomly-picked kit, so two fighters in the same mode can
    // genuinely fight differently between matches, while both sides of any given match are
    // guaranteed an identical, fair loadout - see StartFight/_currentKit.
    private readonly record struct ArenaKit(EntProtoId[] Items, MartialArtStyle Style = MartialArtStyle.None)
    {
        public static implicit operator ArenaKit(EntProtoId[] items) => new(items);
    }

    private static readonly ArenaKit[] MeleeKits =
    {
        new EntProtoId[] { "Katana" },
        new EntProtoId[] { "Machete" },
        new EntProtoId[] { "Cutlass" },
        new EntProtoId[] { "EnergySword" },
        new EntProtoId[] { "EnergyDaggerLoud", "EnergyDaggerLoud" },
        new EntProtoId[] { "KukriKnife", "CombatKnife" },
        new EntProtoId[] { "FireAxe" },
        new EntProtoId[] { "Sledgehammer" },
        new EntProtoId[] { "Chainsaw" },
        new EntProtoId[] { "Spear" },
        new EntProtoId[] { "SpearReinforced" },
        new EntProtoId[] { "Claymore" },
        new EntProtoId[] { "BaseBallBat" },
        new EntProtoId[] { "Stunprod", "CombatKnife" },
        new EntProtoId[] { "ArmBlade" },
        new EntProtoId[] { "Pickaxe" },
        new EntProtoId[] { "MiningDrill" },
        new EntProtoId[] { "WeaponCrusher" },
        new EntProtoId[] { "WeaponCrusherGlaive" },
        new EntProtoId[] { "WeaponCrusherDagger" },
        new EntProtoId[] { "CaptainSabre" },
        new EntProtoId[] { "WeaponMeleeToolboxRobust" },
        new EntProtoId[] { "SurvivalKnife", "ThrowingKnife", "ThrowingKnife" },
        new EntProtoId[] { "EnergyCutlass" },
        new EntProtoId[] { "HyperEutacticBlade" },
        // Martial artists - granted directly (see GiveLoadout), no manual/belt item needed.
        new ArenaKit(Array.Empty<EntProtoId>(), MartialArtStyle.SleepingCarp),
        new ArenaKit(new EntProtoId[] { "CombatKnife" }, MartialArtStyle.CQC),
        new ArenaKit(new EntProtoId[] { "Stunprod" }, MartialArtStyle.CQC),
        new ArenaKit(Array.Empty<EntProtoId>(), MartialArtStyle.Capoeira),
        new ArenaKit(new EntProtoId[] { "KukriKnife" }, MartialArtStyle.Ninjutsu),
        new ArenaKit(Array.Empty<EntProtoId>(), MartialArtStyle.KungFuDragon),
        new ArenaKit(Array.Empty<EntProtoId>(), MartialArtStyle.CorporateJudo),
    };

    private static readonly ArenaKit[] RangedKits =
    {
        new EntProtoId[] { "WeaponRifleAk" },
        new EntProtoId[] { "WeaponSubMachineGunC20r" },
        new EntProtoId[] { "WeaponPistolMk58", "WeaponPistolMk58" },
        new EntProtoId[] { "WeaponShotgunBulldog" },
        new EntProtoId[] { "WeaponShotgunDoubleBarreled" },
        new EntProtoId[] { "WeaponRevolverPython" },
        new EntProtoId[] { "WeaponRevolverDeckard" },
        new EntProtoId[] { "WeaponSubMachineGunDrozd" },
        new EntProtoId[] { "WeaponSubMachineGunWt550" },
        new EntProtoId[] { "WeaponRifleLecter" },
        new EntProtoId[] { "WeaponPistolViper", "WeaponPistolViper" },
        new EntProtoId[] { "WeaponPistolCobra" },
        new EntProtoId[] { "WeaponPistolEchis" },
        new EntProtoId[] { "WeaponShotgunEnforcer" },
        new EntProtoId[] { "WeaponShotgunKammerer" },
        new EntProtoId[] { "WeaponShotgunSawn" },
        new EntProtoId[] { "WeaponRevolverMateba" },
        new EntProtoId[] { "WeaponRevolverInspector" },
        new EntProtoId[] { "WeaponSubMachineGunAtreides" },
        new EntProtoId[] { "WeaponPistolN1984" },
        new EntProtoId[] { "WeaponRifleEstoc" },
        new EntProtoId[] { "WeaponShotgunHushpup" },
        new EntProtoId[] { "WeaponRevolverMateba", "WeaponPistolMk58" },
        new EntProtoId[] { "WeaponRifleAk", "WeaponPistolMk58" },
        new EntProtoId[] { "WeaponSubMachineGunC20r", "WeaponPistolViper" },
    };

    private static readonly ArenaKit[] HybridKits =
    {
        new EntProtoId[] { "WeaponRifleAk", "Katana" },
        new EntProtoId[] { "WeaponSubMachineGunC20r", "CombatKnife" },
        new EntProtoId[] { "WeaponPistolMk58", "Machete" },
        new EntProtoId[] { "WeaponShotgunBulldog", "FireAxe" },
        new EntProtoId[] { "WeaponRevolverPython", "Cutlass" },
        new EntProtoId[] { "WeaponPistolViper", "Spear" },
        new EntProtoId[] { "WeaponSubMachineGunDrozd", "Stunprod" },
        new EntProtoId[] { "WeaponShotgunDoubleBarreled", "Sledgehammer" },
        new EntProtoId[] { "WeaponRevolverDeckard", "KukriKnife" },
        new EntProtoId[] { "WeaponRifleLecter", "EnergyDaggerLoud" },
        new EntProtoId[] { "WeaponPistolCobra", "ArmBlade" },
        new EntProtoId[] { "WeaponShotgunEnforcer", "Claymore" },
        new EntProtoId[] { "WeaponSubMachineGunWt550", "BaseBallBat" },
        new EntProtoId[] { "WeaponRevolverMateba", "Chainsaw" },
        new EntProtoId[] { "WeaponPistolEchis", "SurvivalKnife" },
        new EntProtoId[] { "WeaponShotgunKammerer", "Pickaxe" },
        new EntProtoId[] { "WeaponRevolverInspector", "WeaponMeleeToolboxRobust" },
        new EntProtoId[] { "WeaponSubMachineGunAtreides", "MiningDrill" },
        new EntProtoId[] { "WeaponPistolN1984", "WeaponCrusher" },
        new EntProtoId[] { "WeaponRifleEstoc", "CaptainSabre" },
        new EntProtoId[] { "WeaponShotgunSawn", "EnergySword" },
        new EntProtoId[] { "WeaponShotgunHushpup", "SpearReinforced" },
        new EntProtoId[] { "WeaponRevolverInspector", "Claymore" },
        new EntProtoId[] { "WeaponRevolverPython", "EnergyCutlass" },
        new EntProtoId[] { "WeaponRifleAk", "HyperEutacticBlade" },
    };

    private ArenaState _state = ArenaState.Idle;
    private ArenaMode _mode = ArenaMode.Melee;
    private float _timer;
    private readonly List<NetUserId> _queue = new();
    private readonly HashSet<EntityUid> _alive = new();
    private EntityUid? _arenaMap;

    // 🌇Sunset🌇 - picked ONCE per match (see StartFight) so every fighter gets the identical kit -
    // a fair fight, not a random weapon-tier lottery between the two of them.
    private ArenaKit _currentKit = new(Array.Empty<EntProtoId>());

    public override void Initialize()
    {
        SubscribeNetworkEvent<ArenaCreateRequestEvent>(OnCreateRequest);
        SubscribeNetworkEvent<ArenaJoinRequestEvent>(OnJoinRequest);
        SubscribeNetworkEvent<ArenaSpectateRequestEvent>(OnSpectateRequest);
        SubscribeNetworkEvent<ArenaLeaderboardRequestEvent>(OnLeaderboardRequest); // 🌇Sunset🌇

        SubscribeLocalEvent<ArenaCombatantComponent, MobStateChangedEvent>(OnCombatantStateChanged);
        SubscribeLocalEvent<ArenaCombatantComponent, PlayerDetachedEvent>(OnCombatantDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<KillReportedEvent>(OnKillReported); // 🌇Sunset🌇
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        switch (_state)
        {
            case ArenaState.Queueing:
                _timer -= frameTime;
                if (_timer <= 0f)
                    StartFight();
                break;
            case ArenaState.Cooldown:
                _timer -= frameTime;
                if (_timer <= 0f)
                {
                    _state = ArenaState.Idle;
                    Broadcast();
                }
                break;
        }
    }

    #region Requests

    private void OnCreateRequest(ArenaCreateRequestEvent msg, EntitySessionEventArgs args)
    {
        if (_state != ArenaState.Idle)
            return;

        var session = args.SenderSession;
        if (!IsGhost(session))
            return;

        _mode = msg.Mode;
        _queue.Clear();
        _queue.Add(session.UserId);
        _timer = QueueSeconds;
        _state = ArenaState.Queueing;
        Broadcast();
        AnnounceGathering(session);
    }

    /// <summary>
    /// Pings every other currently-connected ghost with a chat message + sound announcing that an
    /// arena match is gathering and which mode was picked, mirroring how important ghost roles get
    /// announced (see GhostRoleSystem.RegisterGhostRole).
    /// </summary>
    private void AnnounceGathering(ICommonSession creator)
    {
        var modeName = Loc.GetString(_mode switch
        {
            ArenaMode.Melee => "arena-mode-melee",
            ArenaMode.MeleeRanged => "arena-mode-meleeranged",
            ArenaMode.Ranged => "arena-mode-ranged",
            _ => "arena-mode-melee",
        });

        var message = Loc.GetString("arena-gathering-announcement", ("creator", creator.Name), ("mode", modeName));

        foreach (var session in _players.Sessions)
        {
            if (session == creator || !IsGhost(session))
                continue;

            _audio.PlayGlobal(GatherSound, Filter.SinglePlayer(session), false);
            _chat.ChatMessageToOne(ChatChannel.Server, message, message, default, false, session.Channel);
        }
    }

    private void OnJoinRequest(ArenaJoinRequestEvent msg, EntitySessionEventArgs args)
    {
        if (_state != ArenaState.Queueing)
            return;

        var session = args.SenderSession;
        if (!IsGhost(session))
            return;

        // Toggle: pressing the button again leaves the queue (like the lobby "Ready" button).
        if (_queue.Remove(session.UserId))
        {
            // The creator (or last person) leaving cancels the whole match.
            if (_queue.Count == 0)
                _state = ArenaState.Idle;

            Broadcast();
            return;
        }

        if (_queue.Count >= MaxFighters)
            return;

        _queue.Add(session.UserId);
        Broadcast();
    }

    private void OnSpectateRequest(ArenaSpectateRequestEvent msg, EntitySessionEventArgs args)
    {
        if (_state != ArenaState.Fighting || _arenaMap is not { } map)
            return;

        var session = args.SenderSession;
        if (!IsGhost(session) || session.AttachedEntity is not { } ghost)
            return;

        if (!TryComp<ArenaMapComponent>(map, out var arena))
            return;

        _transform.SetCoordinates(ghost, arena.Center);
    }

    // 🌇Sunset🌇
    private void OnLeaderboardRequest(ArenaLeaderboardRequestEvent msg, EntitySessionEventArgs args)
    {
        var session = args.SenderSession;
        var mode = msg.Mode;
        _ = SendLeaderboardAsync(session, mode);
    }

    private async Task SendLeaderboardAsync(ICommonSession session, ArenaMode mode)
    {
        var stats = await _db.GetTopArenaPlayers((int) mode, LeaderboardSize);

        var entries = new List<ArenaLeaderboardEntry>(stats.Count);
        foreach (var stat in stats)
        {
            var record = await _db.GetPlayerRecordByUserId(new NetUserId(stat.PlayerUserId));
            var name = record?.LastSeenUserName ?? stat.PlayerUserId.ToString();
            entries.Add(new ArenaLeaderboardEntry(name, stat.Kills, stat.Deaths, stat.Wins));
        }

        if (session.Channel != null)
            RaiseNetworkEvent(new ArenaLeaderboardResponseEvent(mode, entries), session.Channel);
    }

    #endregion

    #region Fight flow

    private void StartFight()
    {
        // Need at least two fighters for a fight to make sense.
        if (_queue.Count < 2)
        {
            _state = ArenaState.Idle;
            _queue.Clear();
            Broadcast();
            return;
        }

        if (!EnsureArena(out var arena))
        {
            Log.Error("Failed to create the arena map. Aborting match.");
            _state = ArenaState.Idle;
            _queue.Clear();
            Broadcast();
            return;
        }

        var kits = _mode switch
        {
            ArenaMode.Melee => MeleeKits,
            ArenaMode.MeleeRanged => HybridKits,
            ArenaMode.Ranged => RangedKits,
            _ => MeleeKits,
        };
        _currentKit = _random.Pick(kits);

        var points = new List<EntityCoordinates>(arena.Comp.SpawnPoints);
        _random.Shuffle(points);

        _alive.Clear();
        var i = 0;
        foreach (var user in _queue)
        {
            if (i >= points.Count)
                break; // No more spawn points; remaining queued ghosts just don't get in.

            if (SpawnFighter(user, points[i]))
                i++;
        }

        _queue.Clear();

        // Everyone disconnected/left before the fight could start.
        if (_alive.Count < 2)
        {
            CleanupArena();
            _state = ArenaState.Idle;
            Broadcast();
            return;
        }

        _state = ArenaState.Fighting;
        Broadcast();
    }

    private bool SpawnFighter(NetUserId user, EntityCoordinates coords)
    {
        if (!_players.TryGetSessionById(user, out var session))
            return false;

        if (!IsGhost(session))
            return false;

        if (!_mind.TryGetMind(session, out var mindId, out var mind))
            return false;

        var body = Spawn(FighterProto, coords);
        _metaData.SetEntityName(body, Loc.GetString("arena-fighter-name"));
        _stationSpawning.EquipStartingGear(body, new ProtoId<StartingGearPrototype>(FighterGear));
        GiveLoadout(body);

        EnsureComp<KillTrackerComponent>(body); // 🌇Sunset🌇 - lets KillReportedEvent fire so we can credit kills

        var combatant = EnsureComp<ArenaCombatantComponent>(body);
        combatant.User = user;
        // Remember what they owned so we can put them back afterwards (their corpse, real body, or
        // observer ghost). We deliberately do NOT delete it, so they keep the ability to return.
        combatant.OriginalBody = mind.OwnedEntity is { } owned ? GetNetEntity(owned) : null;

        _mind.TransferTo(mindId, body, ghostCheckOverride: true, createGhost: false, mind);

        _alive.Add(body);
        return true;
    }

    private void GiveLoadout(EntityUid body)
    {
        foreach (var proto in _currentKit.Items)
            GiveItem(body, proto);

        if (_currentKit.Style != MartialArtStyle.None)
            _martialArts.TryGrantMartialArt(body, _currentKit.Style);
    }

    private void GiveItem(EntityUid body, EntProtoId proto)
    {
        var item = Spawn(proto, Transform(body).Coordinates);
        _hands.PickupOrDrop(body, item, dropNear: true);
    }

    private void OnCombatantStateChanged(EntityUid uid, ArenaCombatantComponent comp, MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        // 🌇Sunset🌇 - guards against double-processing if gibbing a winner (see CheckForWinner) ever
        // ends up re-raising MobStateChangedEvent for a body FighterOut already removed from play.
        if (!_alive.Contains(uid))
            return;

        RecordArenaResult(comp.User, deaths: 1);

        FighterOut(uid, gib: false);
        CheckForWinner();
    }

    // 🌇Sunset🌇 - credits the killer (not the death itself, see OnCombatantStateChanged) whenever an
    // arena fighter is killed by another currently-active arena fighter.
    private void OnKillReported(ref KillReportedEvent ev)
    {
        if (ev.Suicide || ev.Primary is not KillPlayerSource killer)
            return;

        if (!HasComp<ArenaCombatantComponent>(ev.Entity))
            return;

        if (!_players.TryGetSessionById(killer.PlayerId, out var killerSession)
            || killerSession.AttachedEntity is not { } killerBody
            || !HasComp<ArenaCombatantComponent>(killerBody))
            return;

        RecordArenaResult(killer.PlayerId, kills: 1);
    }

    private void RecordArenaResult(NetUserId user, int kills = 0, int deaths = 0, bool won = false)
    {
        _ = _db.AddArenaMatchResult(user, (int) _mode, kills, deaths, won);
    }

    private void OnCombatantDetached(EntityUid uid, ArenaCombatantComponent comp, PlayerDetachedEvent args)
    {
        // A fighter disconnected or otherwise left their body mid-fight. Treat them as out.
        if (!_alive.Contains(uid))
            return;

        FighterOut(uid, gib: false);
        CheckForWinner();
    }

    private void CheckForWinner()
    {
        if (_state != ArenaState.Fighting)
            return;

        if (_alive.Count > 1)
            return;

        // Last survivor (if any) gets dramatically gibbed, then the arena is torn down. Grab their
        // user id for the win before FighterOut (and the gib) tears the component down.
        foreach (var winner in new List<EntityUid>(_alive))
        {
            if (TryComp<ArenaCombatantComponent>(winner, out var combatant))
                RecordArenaResult(combatant.User, won: true);

            FighterOut(winner, gib: true);
        }

        CleanupArena();
        _state = ArenaState.Cooldown;
        _timer = CooldownSeconds;
        Broadcast();
    }

    /// <summary>
    /// Removes a fighter from the match: returns the player to the body they came from (so they keep
    /// the ability to return to their real body), or to a fresh observer ghost if that body is gone.
    /// Then deletes or gibs the temporary arena body. Idempotent.
    /// </summary>
    private void FighterOut(EntityUid body, bool gib)
    {
        if (!_alive.Remove(body))
            return;

        if (_mind.TryGetMind(body, out var mindId, out var mind))
        {
            EntityUid? original = null;
            if (TryComp<ArenaCombatantComponent>(body, out var combatant)
                && combatant.OriginalBody is { } netOriginal
                && TryGetEntity(netOriginal, out var orig)
                && orig != body)
            {
                original = orig;
            }

            if (original != null)
            {
                // Hand them back their original body/ghost from before the arena.
                _mind.TransferTo(mindId, original.Value, ghostCheckOverride: true, createGhost: false, mind);
            }
            else
            {
                // They had no body to return to: drop them at an observer ghost off-arena.
                var ghost = Spawn(SpectatorGhostProto, _ticker.GetObserverSpawnPoint());
                _ghost.SetCanReturnToBody(ghost, false);
                _mind.TransferTo(mindId, ghost, ghostCheckOverride: true, createGhost: false, mind);
            }
        }

        if (gib && !TerminatingOrDeleted(body))
            _gibbing.Gib(body);
        else
            QueueDel(body);
    }

    #endregion

    #region Arena map

    private bool EnsureArena(out Entity<ArenaMapComponent> arena)
    {
        // Pick a hand-made map sized for the current population if one exists: a small roster gets a
        // tighter arena, a large one gets the bigger layout. Falls back to the single legacy
        // OptionalMapPath, then to fully procedural generation, if the sized map isn't there.
        var populationPath = _players.Sessions.Length <= SmallPopulationThreshold ? SmallMapPath : LargeMapPath;

        if (TryLoadArenaMap(populationPath, out arena))
            return true;

        if (TryLoadArenaMap(OptionalMapPath, out arena))
            return true;

        return GenerateArena(out arena);
    }

    private bool TryLoadArenaMap(string mapPath, out Entity<ArenaMapComponent> arena)
    {
        arena = default;

        if (!_res.ContentFileExists(new ResPath("/" + mapPath))
            || !_mapLoader.TryLoadMap(new ResPath(mapPath), out var loaded, out _,
                new DeserializationOptions { InitializeMaps = true }))
            return false;

        _map.SetPaused(loaded.Value.Comp.MapId, false);
        _arenaMap = loaded.Value.Owner;

        var comp = EnsureComp<ArenaMapComponent>(loaded.Value.Owner);
        comp.SpawnPoints.Clear();
        var query = EntityQueryEnumerator<Components.ArenaSpawnComponent, TransformComponent>();
        while (query.MoveNext(out _, out var xform))
        {
            if (xform.MapUid == loaded.Value.Owner)
                comp.SpawnPoints.Add(xform.Coordinates);
        }

        comp.Center = new EntityCoordinates(loaded.Value.Owner, default);
        if (comp.SpawnPoints.Count >= 2)
        {
            arena = (loaded.Value.Owner, comp);
            return true;
        }

        // Map exists but has too few spawn points; fall through to the next candidate.
        QueueDel(loaded.Value.Owner);
        _arenaMap = null;
        return false;
    }

    /// <summary>
    /// Fallback used until real hand-made maps exist at SmallMapPath/LargeMapPath (see EnsureArena) -
    /// a plain walled floor, sized and populated with more/fewer spawn points depending on the
    /// server's current population, so a quiet server doesn't get a cavernous ring built for 12.
    /// </summary>
    private bool GenerateArena(out Entity<ArenaMapComponent> arena)
    {
        arena = default;

        var small = _players.Sessions.Length <= SmallPopulationThreshold;
        var size = small ? 13 : ArenaSize;
        var spawnCount = small ? Math.Min(MaxFighters, 8) : MaxFighters;

        var mapUid = _map.CreateMap(out var mapId, runMapInit: false);
        var grid = EnsureComp<MapGridComponent>(mapUid);
        var tile = new Tile(_tileDef[FloorTile].TileId);

        for (var x = 0; x < size; x++)
        {
            for (var y = 0; y < size; y++)
            {
                _map.SetTile((mapUid, grid), new Vector2i(x, y), tile);

                // Wall off the perimeter so fighters cannot leave.
                if (x == 0 || y == 0 || x == size - 1 || y == size - 1)
                    Spawn(WallProto, new EntityCoordinates(mapUid, new Vector2(x + 0.5f, y + 0.5f)));
            }
        }

        var comp = EnsureComp<ArenaMapComponent>(mapUid);
        var center = size / 2f;
        comp.Center = new EntityCoordinates(mapUid, new Vector2(center, center));

        var radius = center - 2.5f;
        for (var i = 0; i < spawnCount; i++)
        {
            var angle = MathF.Tau * i / spawnCount;
            var pos = new Vector2(center + radius * MathF.Cos(angle), center + radius * MathF.Sin(angle));
            comp.SpawnPoints.Add(new EntityCoordinates(mapUid, pos));
        }

        _map.InitializeMap(mapId);
        _map.SetPaused(mapId, false);

        _arenaMap = mapUid;
        arena = (mapUid, comp);
        return true;
    }

    private void CleanupArena()
    {
        // Safety: eject anyone still tracked as alive.
        foreach (var body in new List<EntityUid>(_alive))
            FighterOut(body, gib: false);

        if (_arenaMap is { } map && !TerminatingOrDeleted(map))
            QueueDel(map);

        _arenaMap = null;
        _alive.Clear();
    }

    #endregion

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        // The map and bodies are wiped by the round restart; just reset our bookkeeping.
        _state = ArenaState.Idle;
        _queue.Clear();
        _alive.Clear();
        _arenaMap = null;
        _timer = 0f;

        // Clients only ever learn our state from a broadcast ArenaStatusEvent - their own _state
        // field otherwise just keeps whatever it was before the restart (e.g. Fighting), since a
        // round restart doesn't recreate their client-side Arena window/system. Without this,
        // anyone who was mid-match (or queued) when the round restarted would see a stale UI - e.g.
        // stuck on "Spectate" instead of Create/Join - until some unrelated Arena action happened to
        // trigger another broadcast.
        Broadcast();
    }

    private bool IsGhost(ICommonSession session)
    {
        return session.AttachedEntity is { } ent && HasComp<GhostComponent>(ent);
    }

    private void Broadcast()
    {
        var participants = _state == ArenaState.Queueing ? _queue.Count : _alive.Count;

        // Sent per-recipient so each player learns whether they personally are in the queue
        // (used to toggle their Join/Leave button).
        foreach (var session in _players.Sessions)
        {
            if (session.Channel == null)
                continue;

            var inQueue = _state == ArenaState.Queueing && _queue.Contains(session.UserId);
            RaiseNetworkEvent(new ArenaStatusEvent(_state, _mode, participants, _timer, inQueue), session.Channel);
        }
    }
}
