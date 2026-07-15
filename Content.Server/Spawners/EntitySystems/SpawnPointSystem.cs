using Content.Server.GameTicking;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Roles;
using Prometheus;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server.Spawners.EntitySystems;

public sealed partial class SpawnPointSystem : EntitySystem
{
    [Dependency] private GameTicker _gameTicker = default!;
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private StationSystem _stationSystem = default!;
    [Dependency] private StationSpawningSystem _stationSpawning = default!;

    // 🌇Sunset🌇 - which exact coordinates have already been handed to a mob this round. Picking is
    // still random, but biased away from a coordinate already in use when an unused alternative
    // exists, so e.g. simultaneous round-start Assistants don't stack on the identical tile and get
    // shoved out of the room (or clear off the grid) by the physics contact solver once they collide.
    private readonly HashSet<EntityCoordinates> _usedSpawnPoints = new();

    public override void Initialize()
    {
        SubscribeLocalEvent<PlayerSpawningEvent>(OnPlayerSpawning);
        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        _usedSpawnPoints.Clear();
    }

    private void OnPlayerSpawning(PlayerSpawningEvent args)
    {
        if (args.SpawnResult != null)
            return;

        // TODO: Cache all this if it ends up important.
        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();

        while (points.MoveNext(out var uid, out var spawnPoint, out var xform))
        {
            if (args.Station != null && _stationSystem.GetOwningStation(uid, xform) != args.Station)
                continue;

            if (_gameTicker.RunLevel == GameRunLevel.InRound && spawnPoint.SpawnType == SpawnPointType.LateJoin)
            {
                possiblePositions.Add(xform.Coordinates);
            }

            if (_gameTicker.RunLevel != GameRunLevel.InRound &&
                spawnPoint.SpawnType == SpawnPointType.Job &&
                (args.Job == null || spawnPoint.Job == null || spawnPoint.Job == args.Job))
            {
                possiblePositions.Add(xform.Coordinates);
            }
        }

        //starlight start, nukie spawn fix
        if (possiblePositions.Count == 0)
        {
            //so we havent found a valid spawn point
            //try to use a late joiner spawn point exclusively
            //this will most likely always end up being arrivals
            points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
            while ( points.MoveNext(out var uid, out var spawnPoint, out var xform))
            {
                // 🌇Sunset🌇 - this fallback used to have no station filter at all, unlike the primary
                // loop above - on a station missing job-specific spawn points it could hand a player
                // ANY LateJoin marker anywhere in the loaded world (e.g. the Arrivals Terminal), not
                // just ones on their own station.
                if (args.Station != null && _stationSystem.GetOwningStation(uid, xform) != args.Station)
                    continue;

                if (spawnPoint.SpawnType == SpawnPointType.LateJoin)
                {
                    possiblePositions.Add(xform.Coordinates);
                }
            }
        }
        //starlight end

        if (possiblePositions.Count == 0)
        {
            // Ok we've still not returned, but we need to put them /somewhere/.
            // TODO: Refactor gameticker spawning code so we don't have to do this!
            var points2 = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();

            if (points2.MoveNext(out _, out var xform))
            {
                Log.Error($"Unable to pick a valid spawn point, picking random spawner as a backup.\nRunLevel: {_gameTicker.RunLevel} Station: {ToPrettyString(args.Station)} Job: {args.Job}");
                possiblePositions.Add(xform.Coordinates);
            }
            else
            {
                Log.Error($"No spawn points were available!\nRunLevel: {_gameTicker.RunLevel} Station: {ToPrettyString(args.Station)} Job: {args.Job}");
                return;
            }
        }

        // 🌇Sunset🌇 - prefer a coordinate nobody's spawned on yet this round, if one's available,
        // instead of every candidate independently re-rolling the same fixed pool regardless of who
        // else already landed where.
        var unusedPositions = possiblePositions.FindAll(pos => !_usedSpawnPoints.Contains(pos));
        var spawnLoc = _random.Pick(unusedPositions.Count > 0 ? unusedPositions : possiblePositions);
        _usedSpawnPoints.Add(spawnLoc);

        args.SpawnResult = _stationSpawning.SpawnPlayerMob(
            spawnLoc,
            args.Job,
            args.HumanoidCharacterProfile,
            args.Station);
    }
}
