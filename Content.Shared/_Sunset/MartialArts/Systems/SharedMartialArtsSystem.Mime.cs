using Content.Shared.Damage;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Projectiles;
using Content.Shared.Speech.Muting;
using Content.Shared._Sunset.MartialArts.Components;
using Content.Shared._Sunset.MartialArts.Events;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;

namespace Content.Shared._Sunset.MartialArts.Systems;

public sealed partial class SharedMartialArtsSystem
{
    private const string MimeBulletProto = "BulletMime";

    private void InitializeMime()
    {
        SubscribeLocalEvent<MimeAdvancedMimeryComponent, MimeInvisibleBlockadeActionEvent>(OnMimeInvisibleBlockadeAction);
        SubscribeLocalEvent<MimeAdvancedMimeryComponent, MimeFingerGunsActionEvent>(OnMimeFingerGunsAction);
        SubscribeLocalEvent<MimeBulletComponent, ProjectileHitEvent>(OnMimeBulletHit);
    }

    private void OnMimeInvisibleBlockadeAction(Entity<MimeAdvancedMimeryComponent> ent, ref MimeInvisibleBlockadeActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;
        MimeInvisibleWall(ent.Owner);
    }

    /// <summary>
    /// The real Finger Guns (matching Goob Station/Reserve-Station's ActionFingerGuns): a genuine
    /// targeted ranged attack that fires a mimed bullet which can miss, instead of this fork's earlier
    /// melee-combo substitute. Requires an empty hand, same as the original.
    /// </summary>
    private void OnMimeFingerGunsAction(Entity<MimeAdvancedMimeryComponent> ent, ref MimeFingerGunsActionEvent args)
    {
        if (args.Handled || _net.IsClient)
            return;

        if (!_hands.TryGetEmptyHand(ent.Owner, out _))
        {
            _popup.PopupClient(Loc.GetString("martial-arts-mime-finger-guns-need-hand"), ent.Owner, ent.Owner);
            return;
        }

        args.Handled = true;

        var fromMap = _transform.GetMapCoordinates(ent.Owner);
        var toMap = _transform.ToMapCoordinates(args.Target);
        var direction = toMap.Position - fromMap.Position;
        if (direction.LengthSquared() < 0.01f)
            return;

        var bullet = Spawn(MimeBulletProto, fromMap);
        var userVelocity = _physics.GetMapLinearVelocity(ent.Owner);
        _gun.ShootProjectile(bullet, direction, userVelocity, ent.Owner, ent.Owner, 25f);
    }

    private void OnMimeBulletHit(Entity<MimeBulletComponent> ent, ref ProjectileHitEvent args)
    {
        EnsureComp<MutedComponent>(args.Target);
        var tempMute = EnsureComp<TemporaryMuteComponent>(args.Target);
        tempMute.ExpiresAt = _timing.CurTime + ent.Comp.MuteDuration;
    }

    /// <summary>
    /// Goob Station's "Invisible Blockade" - a cooldown-gated action (not a combo trigger, matching
    /// the real thing) that forms a real three-tile invisible wall in front of the user (the same
    /// WallInvisible prototype vanilla Mime's own wall power uses, so it self-despawns after 15 seconds).
    /// Server-only: spawning a new entity from client-predicted action use risks the same "added while
    /// resetting predicted entities" class of bug as reactive component grants - SharedMagicSystem's own
    /// instant-spawn spells follow this exact same _net.IsClient guard.
    /// </summary>
    private void MimeInvisibleWall(EntityUid user)
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
