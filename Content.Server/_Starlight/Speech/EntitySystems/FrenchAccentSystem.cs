using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared._Starlight.Speech;
using Content.Shared.Speech;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class FrenchAccentSystem : EntitySystem
{
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    // 🌇Sunset🌇 - "h" at word start and the "th"->'z/'s digraph shift are English-orthography
    // specific and no-ops on Cyrillic. Replaced with: dropping "х" at word start (French speakers
    // famously can't produce that guttural sound either) and roughening "р" into a throatier,
    // more uvular-sounding cluster (French r is uvular, unlike Russian's alveolar trill).
    [GeneratedRegex(@"(?<!\w)х", RegexOptions.IgnoreCase)]
    private static partial Regex RegexStartH();

    [GeneratedRegex(@"р", RegexOptions.IgnoreCase)]
    private static partial Regex RegexR();

    [GeneratedRegex(@"(?<=\w\w)[!?;:](?!\w)", RegexOptions.IgnoreCase)]
    private static partial Regex RegexSpacePunctuation();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FrenchAccentComponent, AccentGetEvent>(OnAccentGet);
    }

    public SpeechMessage Accentuate(SpeechMessage message, FrenchAccentComponent _)
    {
        message = _replacement.ApplyReplacements(message, "french");

        // replaces х with ' at the start of words (visual only)
        message.Text = RegexStartH().Replace(message.Text, "'");

        // spaces out ! ? : and ;
        message.Text = RegexSpacePunctuation().Replace(message.Text, " $&");

        // roughens р into a throatier рх (visual only)
        message.Text = RegexR().Replace(message.Text, m => char.IsUpper(m.Value[0]) ? "Рх" : "рх");
        return message;
    }

    private void OnAccentGet(EntityUid uid, FrenchAccentComponent component, AccentGetEvent args)
        => args.Message = Accentuate(args.Message, component);
}
