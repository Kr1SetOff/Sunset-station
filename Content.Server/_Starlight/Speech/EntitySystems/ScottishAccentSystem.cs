using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class ScottishAccentSystem : EntitySystem
{
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    // 🌇Sunset🌇 - "-ing"/"and" are English grammar-specific, no-ops on Cyrillic. Replaced with
    // equivalent contracted colloquial Russian, distinct from Southern's set.
    [GeneratedRegex(@"\bэто\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegexEto();

    [GeneratedRegex(@"\bсейчас\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegexSeychas();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ScottishAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, ScottishAccentComponent component, AccentGetEvent args)
    {
        args.Message = _replacement.ApplyReplacements(args.Message, "scottish");

        args.Message.Text = RegexEto().Replace(args.Message.Text, m => PreserveCase(m.Value, "эт"));
        args.Message.Text = RegexSeychas().Replace(args.Message.Text, m => PreserveCase(m.Value, "щас"));
    }

    private static string PreserveCase(string original, string replacement)
    {
        if (string.IsNullOrEmpty(original))
            return replacement;

        if (char.IsUpper(original[0]))
        {
            if (original.Length > 1 && char.IsUpper(original[1]))
                return replacement.ToUpperInvariant();
            return char.ToUpperInvariant(replacement[0]) + replacement[1..];
        }

        return replacement;
    }
}
