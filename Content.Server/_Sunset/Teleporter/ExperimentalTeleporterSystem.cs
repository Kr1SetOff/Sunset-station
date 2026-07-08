using System.Linq;
using System.Numerics;
using Content.Shared._Sunset.Teleporter;
using Content.Shared.Charges.Systems;
using Content.Shared.Coordinates.Helpers;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Interaction.Events;
using Content.Shared.Maps;
using Content.Shared.Tag;
using Robust.Server.Audio;
using Robust.Server.Containers;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Random;

namespace Content.Server._Sunset.Teleporter;

// 🌇Sunset🌇 - ported from Goobstation/Reserve-Station's Content.Server._White.Teleporter.
public sealed class ExperimentalTeleporterSystem : EntitySystem
{
    private static readonly DamageSpecifier LethalDamage = new()
    {
        DamageDict = new() { ["Blunt"] = FixedPoint2.New(1000) },
    };

    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly MapSystem _mapSystem = default!;
    [Dependency] private readonly IEntityManager _entManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedChargesSystem _charges = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly TelefragSystem _telefrag = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ExperimentalTeleporterComponent, UseInHandEvent>(OnUse);
    }

    private void OnUse(EntityUid uid, ExperimentalTeleporterComponent component, UseInHandEvent args)
    {
        if (_charges.IsEmpty(uid)
            || !TryComp<TransformComponent>(args.User, out var xform)
            || (_containerSystem.IsEntityInContainer(args.User)
                && !_containerSystem.TryRemoveFromContainer(args.User)))
            return;

        var ev = new TeleportAttemptEvent(false);
        RaiseLocalEvent(args.User, ref ev);
        if (ev.Cancelled)
            return;

        var oldCoords = xform.Coordinates;
        var range = _random.Next(component.MinTeleportRange, component.MaxTeleportRange);
        var offset = xform.LocalRotation.ToWorldVec().Normalized();
        var direction = xform.LocalRotation.GetDir().ToVec();
        var newOffset = offset + direction * range;

        var coords = xform.Coordinates.Offset(newOffset).SnapToGrid(EntityManager);

        Teleport(args.User, uid, component, coords, oldCoords);

        if (!TryCheckWall(coords)
            || EmergencyTeleportation(args.User, uid, component, xform, oldCoords, newOffset))
            return;

        // Ported without the upstream gib call - this fork's body system doesn't expose one, so a
        // failed emergency teleport into a wall just kills the user outright instead of gibbing them.
        _damageable.TryChangeDamage(args.User, LethalDamage, ignoreResistances: true);
    }

    private bool EmergencyTeleportation(EntityUid uid, EntityUid teleporterUid, ExperimentalTeleporterComponent component, TransformComponent xform, EntityCoordinates oldCoords, Vector2 offset)
    {
        var newOffset = offset + VectorRandomDirection(component, offset, component.EmergencyLength);
        var coords = xform.Coordinates.Offset(newOffset).SnapToGrid(EntityManager);

        if (_charges.IsEmpty(teleporterUid))
            return false;

        Teleport(uid, teleporterUid, component, coords, oldCoords);

        return !TryCheckWall(coords);
    }

    private void Teleport(EntityUid uid, EntityUid teleporterUid, ExperimentalTeleporterComponent component, EntityCoordinates coords, EntityCoordinates oldCoords)
    {
        PlaySoundAndEffects(component, coords, oldCoords);

        _telefrag.DoTelefrag(uid, coords, TimeSpan.Zero);
        _transform.SetCoordinates(uid, coords);

        _charges.TryUseCharge(teleporterUid);
    }

    private void PlaySoundAndEffects(ExperimentalTeleporterComponent component, EntityCoordinates coords, EntityCoordinates oldCoords)
    {
        _audio.PlayPvs(component.TeleportSound, coords);
        _audio.PlayPvs(component.TeleportSound, oldCoords);

        _entManager.SpawnEntity(component.TeleportInEffect, coords);
        _entManager.SpawnEntity(component.TeleportOutEffect, oldCoords);
    }

    private bool TryCheckWall(EntityCoordinates coords)
    {
        if (!_turf.TryGetTileRef(coords, out var tile)
            || !TryComp<MapGridComponent>(tile.Value.GridUid, out var mapGridComponent))
            return false;

        var anchoredEntities = _mapSystem.GetAnchoredEntities(tile.Value.GridUid, mapGridComponent, coords);

        return anchoredEntities.Any(x => _tag.HasTag(x, "Wall"));
    }

    private Vector2 VectorRandomDirection(ExperimentalTeleporterComponent component, Vector2 offset, int length)
    {
        if (component.RandomRotations.Count == 0)
            return Vector2.Zero;

        var randomRotation = _random.Next(0, component.RandomRotations.Count);
        return Angle.FromDegrees(component.RandomRotations[randomRotation]).RotateVec(offset.Normalized() * length);
    }
}
