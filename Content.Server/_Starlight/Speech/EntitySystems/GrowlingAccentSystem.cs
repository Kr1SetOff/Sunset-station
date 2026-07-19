using System.Text.RegularExpressions;
using Content.Shared.Speech;
using Robust.Shared.Random;

namespace Content.Shared._Starlight.Speech.Components;

public sealed partial class GrowlingAccentSystem : EntitySystem
{
    [Dependency] private IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GrowlingAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, GrowlingAccentComponent component, AccentGetEvent args)
    {
        var message = args.Message.Text;

        // 🌇Sunset🌇 - matched Latin r, a no-op on Cyrillic. Cyrillic "р" is already a trilled
        // "rolled" r, making it an even better fit for an exaggerated growl than Latin r was.
        // р => ррр
        message = Regexr().Replace(message, _random.Pick(new List<string> { "рр", "ррр" })
);
        // Р => РРР
        message = RegexR().Replace(message, _random.Pick(new List<string> { "РР", "РРР" })
);

        args.Message.Text = message;
    }

    [GeneratedRegex("р+")]
    private static partial Regex Regexr();
    [GeneratedRegex("Р+")]
    private static partial Regex RegexR();
}
