// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Destructible.Thresholds;
using Content.Shared.Physics;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Shared._Goobstation.Wizard.RandomTeleport;

// Adapted for this fork: matches the already-simplified RandomTeleportComponent (no
// SparksSystem visual effect, no pulled-entity-follows-you behavior - see that component's doc
// comment for why).
[Virtual]
public partial class SharedRandomTeleportSystem : EntitySystem
{
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;

    private EntityQuery<PhysicsComponent> _physicsQuery;

    public override void Initialize()
    {
        base.Initialize();

        _physicsQuery = GetEntityQuery<PhysicsComponent>();
    }

    public bool RandomTeleport(EntityUid target, RandomTeleportComponent rtp, bool sound = true, bool @event = true)
        => RandomTeleport(target, rtp, out _, sound, @event);

    public bool RandomTeleport(EntityUid target, RandomTeleportComponent rtp, out Vector2? finalWorldPos, bool sound = true, bool @event = true)
    {
        finalWorldPos = null;

        if (@event && !CanTeleport(target))
            return false;

        if (sound) _audio.PlayPvs(rtp.DepartureSound, Transform(target).Coordinates, AudioParams.Default);

        finalWorldPos = RandomTeleport(target, rtp.Radius, rtp.TeleportAttempts, rtp.ForceSafeTeleport);

        if (sound) _audio.PlayPvs(rtp.ArrivalSound, Transform(target).Coordinates, AudioParams.Default);

        return true;
    }

    public Vector2 GetTeleportVector(float minRadius, float extraRadius)
    {
        var distance = minRadius + extraRadius * MathF.Sqrt(_random.NextFloat());
        return _random.NextAngle().ToVec() * distance;
    }

    public Vector2? RandomTeleport(EntityUid uid, MinMax radius, int triesBase = 10, bool forceSafe = true)
    {
        var xform = Transform(uid);
        var entityCoords = _xform.ToMapCoordinates(xform.Coordinates);

        var targetCoords = new MapCoordinates();

        var tries = triesBase;

        if (forceSafe) tries *= 2;

        var extraRadiusBase = radius.Max - radius.Min;
        var foundValid = false;
        for (var i = 0; i < tries; i++)
        {
            var extraRadius = extraRadiusBase;
            if (forceSafe && i >= triesBase)
                extraRadius *= (tries - i) / triesBase;

            targetCoords = entityCoords.Offset(GetTeleportVector(radius.Min, extraRadius));

            if (!_mapManager.TryFindGridAt(targetCoords, out var gridUid, out var grid))
                continue;

            var valid = true;
            foreach (var entity in _map.GetAnchoredEntities((gridUid, grid), targetCoords))
            {
                if (!_physicsQuery.TryGetComponent(entity, out var body))
                    continue;

                if (body.BodyType != BodyType.Static || !body.Hard ||
                    (body.CollisionLayer & (int) CollisionGroup.Impassable) == 0)
                    continue;

                valid = false;
                break;
            }

            if (valid)
            {
                foundValid = true;
                break;
            }
        }

        if (!foundValid) targetCoords = entityCoords.Offset(GetTeleportVector(radius.Min, extraRadiusBase));

        var newPos = targetCoords.Position;
        _xform.SetWorldPosition(uid, newPos);

        return newPos;
    }

    private bool CanTeleport(EntityUid uid)
    {
        var ev = new TeleportAttemptEvent(false);
        RaiseLocalEvent(uid, ref ev);
        return !ev.Cancelled;
    }
}
