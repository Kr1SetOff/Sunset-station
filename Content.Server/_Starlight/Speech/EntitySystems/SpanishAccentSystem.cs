using System.Text;
using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Server.Speech;
using Content.Shared._Starlight.Speech;
using Content.Shared.Speech;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class SpanishAccentSystem : EntitySystem
{
    // 🌇Sunset🌇 - "insert e before s" was an English/Spanish-orthography-specific gag
    // (Spanish phonotactics disallow initial s-clusters), meaningless on Cyrillic text. Swapped for
    // an exaggerated rolled "р" - Spanish's other, more universally recognized speech trait.
    [GeneratedRegex("р+")]
    private static partial Regex RegexLowerR();
    [GeneratedRegex("Р+")]
    private static partial Regex RegexUpperR();

    public override void Initialize()
        => SubscribeLocalEvent<SpanishAccentComponent, AccentGetEvent>(OnAccent);

    public SpeechMessage Accentuate(SpeechMessage message)
    {
        message.Text = RegexLowerR().Replace(message.Text, "ррр");
        message.Text = RegexUpperR().Replace(message.Text, "РРР");

        // If a sentence ends with ?, insert a reverse ? at the beginning
        message.Text = ReplacePunctuation(message.Text);

        return message;
    }

    private static string ReplacePunctuation(string message)
    {
        var sentences = AccentSystem.SentenceRegex.Split(message);
        var msg = new StringBuilder();
        foreach (var s in sentences)
        {
            var toInsert = new StringBuilder();
            for (var i = s.Length - 1; i >= 0 && "?!‽".Contains(s[i]); i--)
            {
                toInsert.Append(s[i] switch
                {
                    '?' => '¿',
                    '!' => '¡',
                    '‽' => '⸘',
                    _ => ' '
                });
            }
            if (toInsert.Length == 0)
                msg.Append(s);
            else
                msg.Append(s.Insert(s.Length - s.TrimStart().Length, toInsert.ToString()));
        }
        return msg.ToString();
    }

    private void OnAccent(EntityUid uid, SpanishAccentComponent component, AccentGetEvent args)
        => args.Message = Accentuate(args.Message);
}
