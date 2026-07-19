using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared._Starlight.Speech;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class SkeletonAccentSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    // 🌇Sunset🌇 - the original "-one"->"bone" pun regex was English-spelling specific (it doesn't
    // even produce clean puns in English half the time - "phone" -> "pbone"). There's no clean
    // Cyrillic equivalent that wouldn't risk mangling unrelated words, so it's dropped; the "skeleton"
    // ReplacementAccent dictionary already carries proper Russian bone puns on its own.
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SkeletonAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public SpeechMessage Accentuate(SpeechMessage message, SkeletonAccentComponent component)
    {
        // apply word replacements
        message = _replacement.ApplyReplacements(message, "skeleton");

        // Suffix
        if (_random.Prob(component.ackChance))
        {
            var suffix = " " + Loc.GetString("skeleton-suffix");
            message.Text += suffix;
            message.Tts = (message.Tts ?? message.Text) + suffix;
        }

        return message;
    }

    private void OnAccentGet(EntityUid uid, SkeletonAccentComponent component, AccentGetEvent args)
        => args.Message = Accentuate(args.Message, component);
}
