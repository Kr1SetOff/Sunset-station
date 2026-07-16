using System.Linq;
using Content.Shared._Sunset.Chemistry.Components;
using Content.Shared.EntityEffects;
using Content.Shared.EntityEffects.Effects;
using Content.Shared.Station.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server.EntityEffects.Effects;

/// <summary>
/// 🌇Sunset🌇 - BluespaceLiquid: teleports the metabolizer to a random safe tile on a random station.
/// </summary>
public sealed partial class SunsetRandomTeleportEntityEffectSystem : EntityEffectSystem<TransformComponent, SunsetRandomTeleport>
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly Content.Server.Atmos.EntitySystems.AtmosphereSystem _atmosphere = default!;

    protected override void Effect(Entity<TransformComponent> entity, ref EntityEffectEvent<SunsetRandomTeleport> args)
    {
        var stations = new List<EntityUid>();
        var stationQuery = AllEntityQuery<StationDataComponent>();
        while (stationQuery.MoveNext(out var stationUid, out _))
            stations.Add(stationUid);

        if (stations.Count == 0 || !TryComp<StationDataComponent>(_random.Pick(stations), out var stationData))
            return;

        var grids = new List<Entity<MapGridComponent>>();
        foreach (var grid in stationData.Grids)
        {
            if (TryComp<MapGridComponent>(grid, out var gridComp))
                grids.Add((grid, gridComp));
        }

        if (grids.Count == 0)
            return;

        var (targetGrid, targetGridComp) = _random.Pick(grids);
        var aabb = targetGridComp.LocalAABB;

        for (var i = 0; i < 10; i++)
        {
            var tile = new Vector2i(_random.Next((int) aabb.Left, (int) aabb.Right), _random.Next((int) aabb.Bottom, (int) aabb.Top));

            if (_atmosphere.IsTileSpace(targetGrid, Transform(targetGrid).MapUid, tile) ||
                _atmosphere.IsTileAirBlockedCached(targetGrid, tile))
                continue;

            _transform.SetCoordinates(entity, _map.GridTileToLocal(targetGrid, targetGridComp, tile));
            _transform.AttachToGridOrMap(entity);
            return;
        }
    }
}

/// <summary>
/// 🌇Sunset🌇 - BluespaceDistorter: grants/extends walk-through-everything phasing.
/// </summary>
public sealed partial class SunsetBluespacePhaseEntityEffectSystem : EntityEffectSystem<MetaDataComponent, SunsetBluespacePhase>
{
    [Dependency] private readonly IGameTiming _timing = default!;

    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<SunsetBluespacePhase> args)
    {
        var phase = EnsureComp<BluespacePhaseComponent>(entity);
        phase.EndTime = _timing.CurTime + TimeSpan.FromSeconds(args.Effect.DurationSeconds);
    }
}

/// <summary>
/// 🌇Sunset🌇 - Zapalm: grants/extends the burning star aura.
/// </summary>
public sealed partial class SunsetZapalmAuraEntityEffectSystem : EntityEffectSystem<MetaDataComponent, SunsetZapalmAura>
{
    [Dependency] private readonly IGameTiming _timing = default!;

    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<SunsetZapalmAura> args)
    {
        var aura = EnsureComp<ZapalmAuraComponent>(entity);
        aura.EndTime = _timing.CurTime + TimeSpan.FromSeconds(args.Effect.DurationSeconds);
    }
}
