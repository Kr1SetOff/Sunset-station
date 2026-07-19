using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server.Speech.EntitySystems;

public sealed partial class FrontalLispSystem : EntitySystem
{
    // 🌇Sunset🌇 - the original matched English sibilant spellings (ts/sc/c/ps/z/s/x) that never
    // occur in Cyrillic, making this a complete no-op on Russian speech. A real frontal lisp
    // ("шепелявость") turns с into a softer ш-like sound and з into жь - reproduced here.
    [GeneratedRegex("с+")]
    private static partial Regex RegexLowerS();
    [GeneratedRegex("С+")]
    private static partial Regex RegexUpperS();
    [GeneratedRegex("з+")]
    private static partial Regex RegexLowerZ();
    [GeneratedRegex("З+")]
    private static partial Regex RegexUpperZ();
    [GeneratedRegex("ц+")]
    private static partial Regex RegexLowerTs();
    [GeneratedRegex("Ц+")]
    private static partial Regex RegexUpperTs();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FrontalLispComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, FrontalLispComponent component, AccentGetEvent args)
    {
        var message = args.Message.Text;

        message = RegexLowerZ().Replace(message, "жь");
        message = RegexUpperZ().Replace(message, "Жь");
        message = RegexLowerTs().Replace(message, "фс");
        message = RegexUpperTs().Replace(message, "Фс");
        message = RegexLowerS().Replace(message, "ш");
        message = RegexUpperS().Replace(message, "Ш");

        args.Message.Text = message;
    }
}
