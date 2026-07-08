// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Server._Sunset.Arena.Components;
using Content.Server.GameTicking;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Server.Station.Systems;
using Content.Shared._Sunset.Arena;
using Content.Shared.Gibbing;
using Content.Shared.GameTicking;
using Content.Shared.Ghost;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Mind;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Roles;
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

    private const float QueueSeconds = 20f;
    private const float CooldownSeconds = 5f;
    private const int MaxFighters = 12;
    private const int ArenaSize = 17;

    private const string OptionalMapPath = "Maps/arena.yml";
    private const string FighterGear = "ArenaGear";
    private const string FloorTile = "FloorSteel";

    private static readonly EntProtoId FighterProto = "MobHuman";
    private static readonly EntProtoId WallProto = "WallSolid";
    private static readonly EntProtoId SpectatorGhostProto = "MobObserver";

    private static readonly EntProtoId[] MeleePool =
    {
        "CombatKnife", "Machete", "Cutlass", "Spear", "KukriKnife", "Stunprod",
    };

    private static readonly EntProtoId[] RangedPool =
    {
        "WeaponRifleAk", "WeaponSubMachineGunC20r", "WeaponPistolMk58",
    };

    private ArenaState _state = ArenaState.Idle;
    private ArenaMode _mode = ArenaMode.Melee;
    private float _timer;
    private readonly List<NetUserId> _queue = new();
    private readonly HashSet<EntityUid> _alive = new();
    private EntityUid? _arenaMap;

    public override void Initialize()
    {
        SubscribeNetworkEvent<ArenaCreateRequestEvent>(OnCreateRequest);
        SubscribeNetworkEvent<ArenaJoinRequestEvent>(OnJoinRequest);
        SubscribeNetworkEvent<ArenaSpectateRequestEvent>(OnSpectateRequest);

        SubscribeLocalEvent<ArenaCombatantComponent, MobStateChangedEvent>(OnCombatantStateChanged);
        SubscribeLocalEvent<ArenaCombatantComponent, PlayerDetachedEvent>(OnCombatantDetached);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
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
        switch (_mode)
        {
            case ArenaMode.Melee:
                var count = _random.Next(1, 4); // 1-3 melee weapons
                for (var i = 0; i < count; i++)
                    GiveItem(body, _random.Pick(MeleePool));
                break;
            case ArenaMode.MeleeRanged:
                GiveItem(body, _random.Pick(RangedPool));
                GiveItem(body, _random.Pick(MeleePool));
                break;
            case ArenaMode.Ranged:
                GiveItem(body, _random.Pick(RangedPool));
                GiveItem(body, _random.Pick(RangedPool));
                break;
        }
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

        FighterOut(uid, gib: false);
        CheckForWinner();
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

        // Last survivor (if any) gets dramatically gibbed, then the arena is torn down.
        foreach (var winner in new List<EntityUid>(_alive))
            FighterOut(winner, gib: true);

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
        arena = default;

        // Prefer a hand-made map if one was placed at Resources/Maps/arena.yml.
        if (_res.ContentFileExists(new ResPath("/" + OptionalMapPath))
            && _mapLoader.TryLoadMap(new ResPath(OptionalMapPath), out var loaded, out _,
                new DeserializationOptions { InitializeMaps = true }))
        {
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

            // Map exists but has too few spawn points; fall through to generation.
            QueueDel(loaded.Value.Owner);
            _arenaMap = null;
        }

        return GenerateArena(out arena);
    }

    private bool GenerateArena(out Entity<ArenaMapComponent> arena)
    {
        arena = default;

        var mapUid = _map.CreateMap(out var mapId, runMapInit: false);
        var grid = EnsureComp<MapGridComponent>(mapUid);
        var tile = new Tile(_tileDef[FloorTile].TileId);

        for (var x = 0; x < ArenaSize; x++)
        {
            for (var y = 0; y < ArenaSize; y++)
            {
                _map.SetTile((mapUid, grid), new Vector2i(x, y), tile);

                // Wall off the perimeter so fighters cannot leave.
                if (x == 0 || y == 0 || x == ArenaSize - 1 || y == ArenaSize - 1)
                    Spawn(WallProto, new EntityCoordinates(mapUid, new Vector2(x + 0.5f, y + 0.5f)));
            }
        }

        var comp = EnsureComp<ArenaMapComponent>(mapUid);
        var center = ArenaSize / 2f;
        comp.Center = new EntityCoordinates(mapUid, new Vector2(center, center));

        var radius = center - 2.5f;
        for (var i = 0; i < MaxFighters; i++)
        {
            var angle = MathF.Tau * i / MaxFighters;
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
