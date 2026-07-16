using Content.Server.Chat.Managers;
using Content.Server.GameTicking.Rules;
using Content.Shared._Sunset.Voidwalker;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Station;
using Content.Shared.Station.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Server.Player;
using Robust.Shared.Maths;

namespace Content.Server._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - companion system for the VoidwalkerSpawn game rule. The rule itself is pure YAML
/// (SpaceSpawnRule + AntagSpawner + AntagSelection, same two-hop raffle-spawner pattern as
/// DragonSpawn - see Resources/Prototypes/_Sunset/Voidwalker/game_rule.yml); this system handles
/// what happens once a player is actually inside a Voidwalker regardless of which path put them
/// there (rule raffle, admin "Make Voidwalker" verb, or a plain entity-spawn-panel spawn):
/// assigns tg's flavor objective and tells them which way the station is.
/// </summary>
public sealed class VoidwalkerSpawnRuleSystem : GameRuleSystem<VoidwalkerSpawnRuleComponent>
{
    [Dependency] private readonly SharedStationSystem _station = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    [Dependency] private readonly VoidwalkerSystem _voidwalker = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;

    private const float DefaultSpawnDistance = 20f;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<VoidwalkerComponent, MindAddedMessage>(OnVoidwalkerMindAdded);
    }

    /// <summary>
    /// Moves an already-spawned Voidwalker out into open space near a random eligible station and
    /// records that station on it (VoidwalkerComponent.SpawnStation). Used by the admin "Make
    /// Voidwalker" verb; the game rule's own placement is handled by SpaceSpawnRule instead.
    /// </summary>
    public bool PlaceInSpaceNearStation(EntityUid voidwalker, float spawnDistance = DefaultSpawnDistance)
    {
        if (!TryGetRandomStation(out var station))
            return false;

        var gridUid = _station.GetLargestGrid(station.Value);
        if (gridUid == null || !TryComp<MapGridComponent>(gridUid, out var grid))
            return false;

        // A random angle, at a distance past the station's own bounding radius so it doesn't land
        // inside a wall - same placement math as the engine's own SpaceSpawnRule.
        var radius = grid.LocalAABB.Size.Length() / 2;
        var distance = radius + spawnDistance;
        var angle = RobustRandom.NextAngle();
        var offset = angle.ToVec() * distance;

        var gridXform = Transform(gridUid.Value);
        var position = _transform.GetWorldPosition(gridXform) + offset;
        var mapCoords = new MapCoordinates(position, gridXform.MapID);

        _transform.SetMapCoordinates(voidwalker, mapCoords);
        _voidwalker.SetSpawnStation(voidwalker, station.Value);
        return true;
    }

    private void OnVoidwalkerMindAdded(EntityUid uid, VoidwalkerComponent comp, MindAddedMessage args)
    {
        // tg's actual voidwalker objective: flavor text whose completion check is just staying
        // alive. Added here rather than via AntagObjectives on the rule so every spawn path gets
        // exactly one copy of it.
        if (args.Mind.Comp.Objectives.Count == 0)
            _mind.TryAddObjective(args.Mind.Owner, args.Mind.Comp, "VoidwalkerObjective");

        if (args.Mind.Comp.UserId is not { } userId || !_playerManager.TryGetSessionById(userId, out var session))
            return;

        // Prefer the station recorded at placement time (admin verb path); fall back to whichever
        // station grid is nearest on this map (rule/raffle path, entity-spawn-panel spawns).
        var stationGrid = comp.SpawnStation is { } station && Exists(station)
            ? _station.GetLargestGrid(station)
            : FindNearestStationGrid(uid);

        if (stationGrid == null)
            return;

        var toStation = _transform.GetWorldPosition(stationGrid.Value) - _transform.GetWorldPosition(uid);
        if (toStation.LengthSquared() < 0.01f)
            return;

        var direction = Angle.FromWorldVec(toStation).GetCardinalDir() switch
        {
            Direction.North => "voidwalker-spawn-direction-north",
            Direction.South => "voidwalker-spawn-direction-south",
            Direction.East => "voidwalker-spawn-direction-east",
            _ => "voidwalker-spawn-direction-west",
        };

        _chat.DispatchServerMessage(session, Loc.GetString("voidwalker-spawn-direction", ("direction", Loc.GetString(direction))));
    }

    private EntityUid? FindNearestStationGrid(EntityUid voidwalker)
    {
        var mapId = Transform(voidwalker).MapID;
        var pos = _transform.GetWorldPosition(voidwalker);

        EntityUid? nearest = null;
        var nearestDistance = float.MaxValue;

        var query = AllEntityQuery<StationDataComponent>();
        while (query.MoveNext(out var stationUid, out var data))
        {
            if (_station.GetLargestGrid((stationUid, data)) is not { } grid || Transform(grid).MapID != mapId)
                continue;

            var distance = (_transform.GetWorldPosition(grid) - pos).LengthSquared();
            if (distance >= nearestDistance)
                continue;

            nearestDistance = distance;
            nearest = grid;
        }

        return nearest;
    }
}
