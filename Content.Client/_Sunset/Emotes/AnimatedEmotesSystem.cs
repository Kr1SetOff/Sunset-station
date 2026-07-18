using System.Numerics;
using Content.Shared._Sunset.Emotes;
using Robust.Client.Animations;
using Robust.Client.GameObjects;
using Robust.Shared.Animations;
using Robust.Shared.GameStates;
using Robust.Shared.Maths;

namespace Content.Client._Sunset.Emotes;

/// <summary>
/// 🌇Sunset🌇 - client half of the animated emotes: plays a short sprite animation (offset and/or
/// rotation keyframes) whenever the server stamps an emote id onto AnimatedEmotesComponent.
/// </summary>
public sealed class AnimatedEmotesSystem : EntitySystem
{
    [Dependency] private readonly AnimationPlayerSystem _anim = default!;

    private const string AnimationKey = "sunset-emote";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, AfterAutoHandleStateEvent>(OnHandleState);
    }

    private void OnHandleState(Entity<AnimatedEmotesComponent> ent, ref AfterAutoHandleStateEvent args)
    {
        if (ent.Comp.Emote is not { } emote)
            return;

        // Spin/dance cycle the mob's facing (LocalRotation), so they must start from wherever the
        // mob is currently looking - absolute keyframes would snap everyone to south first.
        var facing = Transform(ent).LocalRotation;

        var animation = emote.Id switch
        {
            "SunsetEmoteJump" => BuildJump(),
            "SunsetEmoteSpin" => BuildSpin(facing),
            "SunsetEmoteDance" => BuildDance(facing),
            "SunsetEmoteFlip" => BuildFlip(1),
            "SunsetEmoteDoubleFlip" => BuildFlip(2),
            _ => null,
        };

        if (animation == null || _anim.HasRunningAnimation(ent, AnimationKey))
            return;

        _anim.Play(ent.Owner, animation, AnimationKey);
    }

    private static Animation BuildJump()
    {
        return new Animation
        {
            Length = TimeSpan.FromSeconds(0.5),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0.35f), 0.25f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.25f),
                    },
                },
            },
        };
    }

    // IMPORTANT: the animation system interpolates Angle keyframes along the SHORTEST arc
    // (Angle.Lerp -> ShortestDistance). Adjacent keyframes therefore must be less than 180 degrees
    // apart or the rotation collapses/reverses - full-turn (2pi) steps don't move at all, and pi
    // steps flip forward-then-backward. Every builder below sticks to 90 degree increments.

    /// <summary>
    /// Spin: the mob rapidly cycles its facing north-east-south-west (the classic SS13 spin),
    /// implemented by animating the transform's LocalRotation in 90 degree steps. Three quick
    /// full cycles, starting and ending at the mob's current facing.
    /// </summary>
    private static Animation BuildSpin(Angle facing)
    {
        const float step = 0.09f;
        const int cycles = 3;

        var track = new AnimationTrackComponentProperty
        {
            ComponentType = typeof(TransformComponent),
            Property = nameof(TransformComponent.LocalRotation),
            InterpolationMode = AnimationInterpolationMode.Linear,
            KeyFrames = { new AnimationTrackProperty.KeyFrame(facing, 0f) },
        };

        for (var i = 1; i <= cycles * 4; i++)
            track.KeyFrames.Add(new AnimationTrackProperty.KeyFrame(facing + new Angle(MathHelper.PiOver2 * i), step));

        return new Animation
        {
            Length = TimeSpan.FromSeconds(step * cycles * 4),
            AnimationTracks = { track },
        };
    }

    /// <summary>
    /// Flip(s): full forward rolls of the sprite texture, 90 degrees per keyframe so a double flip
    /// is really two complete rotations in the same direction.
    /// </summary>
    private static Animation BuildFlip(int flips)
    {
        const float perFlip = 0.5f;
        var track = new AnimationTrackComponentProperty
        {
            ComponentType = typeof(SpriteComponent),
            Property = nameof(SpriteComponent.Rotation),
            InterpolationMode = AnimationInterpolationMode.Linear,
            KeyFrames = { new AnimationTrackProperty.KeyFrame(Angle.Zero, 0f) },
        };

        for (var i = 1; i <= flips * 4; i++)
            track.KeyFrames.Add(new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.PiOver2 * i), perFlip / 4f));

        return new Animation
        {
            Length = TimeSpan.FromSeconds(perFlip * flips),
            AnimationTracks = { track },
        };
    }

    /// <summary>
    /// Dance: tip over onto the head (sprite rotates to 180 and stays there), spin the facing
    /// north-east-south-west twice while upside down - same motion as the spin emote - then flip
    /// back up onto the feet.
    /// </summary>
    private static Animation BuildDance(Angle facing)
    {
        const float tipStep = 0.15f; // 2 x 90 degrees down, 2 x 90 degrees back up
        const float spinStep = 0.12f;
        const int spinCycles = 2;

        const float spinTotal = spinStep * spinCycles * 4; // 0.96s upside down
        const float total = tipStep * 4 + spinTotal; // 1.56s

        var spriteTrack = new AnimationTrackComponentProperty
        {
            ComponentType = typeof(SpriteComponent),
            Property = nameof(SpriteComponent.Rotation),
            InterpolationMode = AnimationInterpolationMode.Linear,
            KeyFrames =
            {
                new AnimationTrackProperty.KeyFrame(Angle.Zero, 0f),
                new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.PiOver2), tipStep),
                new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.Pi), tipStep),
                // Hold upside down while the facing spin runs.
                new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.Pi), spinTotal),
                new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.Pi + MathHelper.PiOver2), tipStep),
                new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.TwoPi), tipStep),
            },
        };

        var facingTrack = new AnimationTrackComponentProperty
        {
            ComponentType = typeof(TransformComponent),
            Property = nameof(TransformComponent.LocalRotation),
            InterpolationMode = AnimationInterpolationMode.Linear,
            KeyFrames =
            {
                new AnimationTrackProperty.KeyFrame(facing, 0f),
                // Wait for the tip-over before spinning.
                new AnimationTrackProperty.KeyFrame(facing, tipStep * 2),
            },
        };

        for (var i = 1; i <= spinCycles * 4; i++)
            facingTrack.KeyFrames.Add(new AnimationTrackProperty.KeyFrame(facing + new Angle(MathHelper.PiOver2 * i), spinStep));

        // Hold the final facing while flipping back onto the feet.
        facingTrack.KeyFrames.Add(new AnimationTrackProperty.KeyFrame(facing, tipStep * 2));

        return new Animation
        {
            Length = TimeSpan.FromSeconds(total),
            AnimationTracks = { spriteTrack, facingTrack },
        };
    }
}
