using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Speech;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class SouthernAccentSystem : EntitySystem
{
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    // 🌇Sunset🌇 - "-ing"/"and"/"d've" are English grammar-specific, no-ops on Cyrillic. Replaced
    // with the equivalent drawled/dropped-letter contractions in colloquial Russian.
    [GeneratedRegex(@"\bчто\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegexChto();

    [GeneratedRegex(@"\bтебя\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegexTebya();

    [GeneratedRegex(@"\bговорю\b", RegexOptions.IgnoreCase)]
    private static partial Regex RegexGovoryu();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SouthernAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, SouthernAccentComponent component, AccentGetEvent args)
    {
        args.Message = _replacement.ApplyReplacements(args.Message, "southern");

        args.Message.Text = RegexChto().Replace(args.Message.Text, m => PreserveCase(m.Value, "чё"));
        args.Message.Text = RegexTebya().Replace(args.Message.Text, m => PreserveCase(m.Value, "тя"));
        args.Message.Text = RegexGovoryu().Replace(args.Message.Text, m => PreserveCase(m.Value, "грю"));
    }

    private static string PreserveCase(string original, string replacement)
    {
        if (string.IsNullOrEmpty(original))
            return replacement;

        if (char.IsUpper(original[0]))
        {
            return original.Length > 1 && char.IsUpper(original[1])
                ? replacement.ToUpperInvariant()
                : char.ToUpperInvariant(replacement[0]) + replacement[1..];
        }

        return replacement;
    }
}
