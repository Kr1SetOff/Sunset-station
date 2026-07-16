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

        var animation = emote.Id switch
        {
            "SunsetEmoteJump" => BuildJump(),
            "SunsetEmoteSpin" => BuildSpin(),
            "SunsetEmoteDance" => BuildDance(),
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

    private static Animation BuildSpin()
    {
        // Three fast full turns, like a spinning top.
        return new Animation
        {
            Length = TimeSpan.FromSeconds(1.2),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.TwoPi), 0.4f),
                        new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.TwoPi * 2), 0.4f),
                        new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.TwoPi * 3), 0.4f),
                    },
                },
            },
        };
    }

    private static Animation BuildFlip(int flips)
    {
        var perFlip = 0.5f;
        var track = new AnimationTrackComponentProperty
        {
            ComponentType = typeof(SpriteComponent),
            Property = nameof(SpriteComponent.Rotation),
            InterpolationMode = AnimationInterpolationMode.Linear,
            KeyFrames = { new AnimationTrackProperty.KeyFrame(Angle.Zero, 0f) },
        };

        for (var i = 1; i <= flips * 2; i++)
            track.KeyFrames.Add(new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.Pi * i), perFlip / 2f));

        return new Animation
        {
            Length = TimeSpan.FromSeconds(perFlip * flips),
            AnimationTracks = { track },
        };
    }

    private static Animation BuildDance()
    {
        // A headstand hop: jump up, land on the head (half turn), bounce twice upside down while
        // finishing two full spins, then land back on the feet.
        return new Animation
        {
            Length = TimeSpan.FromSeconds(1.6),
            AnimationTracks =
            {
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Rotation),
                    InterpolationMode = AnimationInterpolationMode.Linear,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Angle.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.Pi), 0.4f),
                        new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.TwoPi), 0.4f),
                        new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.TwoPi + MathHelper.Pi), 0.4f),
                        new AnimationTrackProperty.KeyFrame(new Angle(MathHelper.TwoPi * 2), 0.4f),
                    },
                },
                new AnimationTrackComponentProperty
                {
                    ComponentType = typeof(SpriteComponent),
                    Property = nameof(SpriteComponent.Offset),
                    InterpolationMode = AnimationInterpolationMode.Cubic,
                    KeyFrames =
                    {
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0.3f), 0.4f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.4f),
                        new AnimationTrackProperty.KeyFrame(new Vector2(0f, 0.3f), 0.4f),
                        new AnimationTrackProperty.KeyFrame(Vector2.Zero, 0.4f),
                    },
                },
            },
        };
    }
}
