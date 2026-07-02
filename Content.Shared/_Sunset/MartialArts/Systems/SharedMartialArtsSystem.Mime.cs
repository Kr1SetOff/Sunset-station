using Content.Shared.Damage;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Speech.Muting;
using Content.Shared._Sunset.MartialArts.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._Sunset.MartialArts.Systems;

public sealed partial class SharedMartialArtsSystem
{
    private void InitializeMime()
    {
    }

    /// <summary>
    /// Goob Station's "Invisible Blockade" - forms a real three-tile invisible wall in front of the
    /// user (the same WallInvisible prototype vanilla Mime's own wall power uses, so it self-despawns
    /// after 15 seconds). Server-only: spawning a new entity from a client-predicted combo trigger risks
    /// the same "added while resetting predicted entities" class of bug as reactive component grants -
    /// SharedMagicSystem's own instant-spawn spells follow this exact same _net.IsClient guard.
    /// </summary>
    private void MimeInvisibleWall(EntityUid user, EntityUid target)
    {
        if (_net.IsClient)
            return;

        var xform = Transform(user);
        if (xform.GridUid is not { } gridUid || !TryComp<MapGridComponent>(gridUid, out var grid))
            return;

        var directionPos = xform.Coordinates.Offset(xform.LocalRotation.ToWorldVec().Normalized());
        if (!_turf.TryGetTileRef(directionPos, out var tileRef))
            return;

        var tileIndex = tileRef.Value.GridIndices;
        var perpendicular = xform.LocalRotation.GetCardinalDir() is Direction.North or Direction.South
            ? new Vector2i(1, 0)
            : new Vector2i(0, 1);

        Spawn("WallInvisible", _mapSystem.GridTileToLocal(gridUid, grid, tileIndex));
        Spawn("WallInvisible", _mapSystem.GridTileToLocal(gridUid, grid, tileIndex + perpendicular));
        Spawn("WallInvisible", _mapSystem.GridTileToLocal(gridUid, grid, tileIndex - perpendicular));
    }

    /// <summary>
    /// Goob Station's "Finger Guns" - mimed bullets that deal Piercing damage and mute on hit. The real
    /// version fires up to three separate shots that can miss; ours is a guaranteed single burst against
    /// the already-selected combo target, so the numbers are scaled down from its 40-per-bullet/20s mute.
    /// </summary>
    private void MimeFingerGuns(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Piercing", 15 } } }, origin: user);

        EnsureComp<MutedComponent>(target);
        var tempMute = EnsureComp<TemporaryMuteComponent>(target);
        tempMute.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(8);
    }

    /// <summary>
    /// Sunset original, not part of Goob Station's Advanced Mimery - mimes an invisible box around the
    /// target and slams the lid. Only really works on someone you've already got a hold of.
    /// </summary>
    private void MimeBoxTrap(EntityUid user, EntityUid target)
    {
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, user);

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(3), force: true);
        _stamina.TakeStaminaDamage(target, 30f, source: user);
    }

    /// <summary>
    /// Sunset original, not part of Goob Station's Advanced Mimery - a big, theatrical, exaggerated slap,
    /// the payoff move once the target is already reeling.
    /// </summary>
    private void MimeExaggeratedSlap(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 15 } } }, origin: user);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(1), force: true);
    }
}
