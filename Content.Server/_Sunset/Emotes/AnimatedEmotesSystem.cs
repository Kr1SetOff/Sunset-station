using Content.Shared._Sunset.Emotes;
using Content.Shared.Chat;

namespace Content.Server._Sunset.Emotes;

/// <summary>
/// 🌇Sunset🌇 - server half of the animated emotes: when a chat emote with one of our ids fires on
/// an entity carrying AnimatedEmotesComponent, stamp it on the (networked) component so every
/// client in PVS plays the animation. See the client-side AnimatedEmotesSystem for the animations.
/// </summary>
public sealed class AnimatedEmotesSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AnimatedEmotesComponent, EmoteEvent>(OnEmote);
    }

    private void OnEmote(Entity<AnimatedEmotesComponent> ent, ref EmoteEvent args)
    {
        switch (args.Emote.ID)
        {
            case "SunsetEmoteJump":
            case "SunsetEmoteSpin":
            case "SunsetEmoteDance":
            case "SunsetEmoteFlip":
            case "SunsetEmoteDoubleFlip":
                ent.Comp.Emote = args.Emote.ID;
                ent.Comp.Counter++;
                Dirty(ent);
                break;
        }
    }
}
