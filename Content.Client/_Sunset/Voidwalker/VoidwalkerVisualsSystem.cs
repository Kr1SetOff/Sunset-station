using System.Numerics;
using Content.Shared._Sunset.Voidwalker;
using Robust.Client.GameObjects;
using Robust.Shared.Maths;
using Robust.Shared.Physics.Components;
using Robust.Shared.Timing;

namespace Content.Client._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - client-side half of the Voidwalker's space stealth (near-invisible while floating
/// in open space - no grid underneath, fully visible everywhere else, see VoidwalkerComponent's
/// SpaceAlpha/NonSpaceAlpha fields) plus a small procedural bob while moving. The ported sprite
/// (tg's icons/mob/simple/voidwalker.dmi) only has one static frame per direction - no walk-cycle
/// exists in the art at all - so this is a code-only stand-in to keep it from reading as a single
/// picture sliding around, not a real replacement for an actual animation.
/// </summary>
public sealed class VoidwalkerVisualsSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private const float BobFrequency = 9f; // radians/second
    private const float BobAmplitude = 0.08f; // world units
    private const float MovingThreshold = 0.01f; // linear velocity squared

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<VoidwalkerComponent, SpriteComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var voidwalker, out var sprite, out var xform))
        {
            var alpha = xform.GridUid == null ? voidwalker.SpaceAlpha : voidwalker.NonSpaceAlpha;
            if (!MathHelper.CloseTo(sprite.Color.A, alpha))
                sprite.Color = sprite.Color.WithAlpha(alpha);

            var isMoving = TryComp<PhysicsComponent>(uid, out var physics) &&
                           physics.LinearVelocity.LengthSquared() > MovingThreshold;
            var bobY = isMoving ? MathF.Sin((float) _timing.CurTime.TotalSeconds * BobFrequency) * BobAmplitude : 0f;

            if (!MathHelper.CloseTo(sprite.Offset.Y, bobY))
                sprite.Offset = new Vector2(sprite.Offset.X, bobY);
        }
    }
}
