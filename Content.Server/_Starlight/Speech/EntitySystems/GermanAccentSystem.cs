using System.Text;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared._Starlight.Speech;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class GermanAccentSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    // 🌇Sunset🌇 - the original "the"/"th" matching and the umlaut sprinkle (which only perturbed
    // Latin A/O/U) were all no-ops on Cyrillic text. Replaced with: occasionally swapping "это" for
    // the German-flavored "дас" (same joke, same 30% chance), в->ф devoicing (a real, very
    // recognizable German-accent trait - "ve have vays" but for Russian's в), and elongating
    // stressed-sounding vowels in place of the umlaut sprinkle.
    [GeneratedRegex(@"(?<=\s|^)это(?=\s|$)", RegexOptions.IgnoreCase)]
    private static partial Regex RegexEto();

    public override void Initialize()
        => SubscribeLocalEvent<GermanAccentComponent, AccentGetEvent>(OnAccent);

    public SpeechMessage Accentuate(SpeechMessage message)
    {
        // rarely, "это" should become "дас"
        message.Text = RegexEto().Replace(message.Text, m => _random.Prob(0.3f)
            ? (char.IsUpper(m.Value[0]) ? "Дас" : "дас")
            : m.Value);

        // apply word replacements
        message = _replacement.ApplyReplacements(message, "german");

        // в -> ф devoicing (visual only for msg, TTS can handle it)
        var msgBuilder = new StringBuilder(message.Text
            .Replace('в', 'ф')
            .Replace('В', 'Ф'));

        // Random vowel elongation, standing in for the umlaut sprinkle (visual only)
        var vowelCooldown = 0;
        for (var i = 0; i < msgBuilder.Length; i++)
        {
            if (vowelCooldown == 0)
            {
                if (_random.Prob(0.1f) && "аоуАОУ".Contains(msgBuilder[i]))
                {
                    msgBuilder.Insert(i, msgBuilder[i]);
                    i++;
                    vowelCooldown = 4;
                }
            }
            else
            {
                vowelCooldown--;
            }
        }

        message.Text = msgBuilder.ToString();
        return message;
    }

    private void OnAccent(Entity<GermanAccentComponent> ent, ref AccentGetEvent args)
        => args.Message = Accentuate(args.Message);
}
