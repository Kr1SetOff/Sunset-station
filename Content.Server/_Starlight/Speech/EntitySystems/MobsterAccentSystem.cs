using System.Linq;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared._Starlight.Speech;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class MobsterAccentSystem : EntitySystem
{
    // 🌇Sunset🌇 - "-ing"/"or"/"ar" are English-spelling specific, no-ops on Cyrillic. Replaced with
    // dropping mid-word "р" after о/а - a mumbled, non-rhotic swallowed-r effect (the same idea as
    // "or"->"uh"/"ar"->"ah" losing the r sound) that fits a gangster mumble in Russian too.
    [GeneratedRegex(@"(?<=\w)ор(?=\w)")]
    private static partial Regex RegexLowerOr();

    [GeneratedRegex(@"(?<=\w)ОР(?=\w)")]
    private static partial Regex RegexUpperOr();

    [GeneratedRegex(@"(?<=\w)ар(?=\w)")]
    private static partial Regex RegexLowerAr();

    [GeneratedRegex(@"(?<=\w)АР(?=\w)")]
    private static partial Regex RegexUpperAr();

    [GeneratedRegex(@"^(\S+)")]
    private static partial Regex RegexFirstWord();

    [GeneratedRegex(@"(\S+)$")]
    private static partial Regex RegexLastWord();

    [GeneratedRegex(@"([.!?]+$)(?!.*[.!?])|(?<![.!?])$")]
    private static partial Regex RegexLastPunctuation();

    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MobsterAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public SpeechMessage Accentuate(SpeechMessage message, MobsterAccentComponent component)
    {
        message = _replacement.ApplyReplacements(message, "mobster");

        // ор -> о and ар -> а (dropped mid-word р)
        message.Text = RegexLowerOr().Replace(message.Text, "о");
        message.Tts = RegexLowerOr().Replace(message.Tts ?? message.Text, "о");

        message.Text = RegexUpperOr().Replace(message.Text, "О");
        message.Tts = RegexUpperOr().Replace(message.Tts ?? message.Text, "О");

        message.Text = RegexLowerAr().Replace(message.Text, "а");
        message.Tts = RegexLowerAr().Replace(message.Tts ?? message.Text, "а");

        message.Text = RegexUpperAr().Replace(message.Text, "А");
        message.Tts = RegexUpperAr().Replace(message.Tts ?? message.Text, "А");

        // Prefix
        if (_random.Prob(0.15f))
        {
            var firstWordAllCaps = !RegexFirstWord().Match(message.Text).Value.Any(char.IsLower);
            var pick = _random.Next(1, 2);
            var prefix = Loc.GetString($"accent-mobster-prefix-{pick}");

            if (!firstWordAllCaps)
            {
                message.Text = message.Text[0].ToString().ToLower() + message.Text[1..];
                message.Tts = (message.Tts ?? message.Text)[0].ToString().ToLower() + (message.Tts ?? message.Text)[1..];
            }
            else
            {
                prefix = prefix.ToUpper();
            }

            message.Text = prefix + " " + message.Text;
            message.Tts = prefix + " " + (message.Tts ?? message.Text);
        }

        message.Text = message.Text[0].ToString().ToUpper() + message.Text[1..];
        message.Tts = (message.Tts ?? message.Text)[0].ToString().ToUpper() + (message.Tts ?? message.Text)[1..];

        // Suffixes
        if (_random.Prob(0.4f))
        {
            var lastWordAllCaps = !RegexLastWord().Match(message.Text).Value.Any(char.IsLower);
            var suffix = component.IsBoss
                ? Loc.GetString($"accent-mobster-suffix-boss-{_random.Next(1, 4)}")
                : Loc.GetString($"accent-mobster-suffix-minion-{_random.Next(1, 3)}");

            if (lastWordAllCaps)
                suffix = suffix.ToUpper();

            message.Text = RegexLastPunctuation().Replace(message.Text, suffix);
            message.Tts = RegexLastPunctuation().Replace(message.Tts ?? message.Text, suffix);
        }

        return message;
    }

    private void OnAccentGet(EntityUid uid, MobsterAccentComponent component, AccentGetEvent args)
        => args.Message = Accentuate(args.Message, component);
}
