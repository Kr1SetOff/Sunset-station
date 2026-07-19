using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class LizardAccentSystem : EntitySystem
{
    // 🌇Sunset🌇 - the original matched Latin s/x, which never occur in Cyrillic text and made this
    // accent a complete no-op on Russian speech. с/ш are Russian's hissing sibilants, so elongating
    // them reproduces the same "snake hiss" effect.
    [GeneratedRegex("с+")]
    private static partial Regex RegexLowerS();

    [GeneratedRegex("С+")]
    private static partial Regex RegexUpperS();

    [GeneratedRegex("ш+")]
    private static partial Regex RegexLowerSh();

    [GeneratedRegex("Ш+")]
    private static partial Regex RegexUpperSh();

    [GeneratedRegex("щ+")]
    private static partial Regex RegexLowerShch();

    [GeneratedRegex("Щ+")]
    private static partial Regex RegexUpperShch();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LizardAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, LizardAccentComponent component, AccentGetEvent args)
    {
        // сссс
        args.Message.Text = RegexLowerS().Replace(args.Message.Text, "ссс");
        // ССС
        args.Message.Text = RegexUpperS().Replace(args.Message.Text, "ССС");
        // шшшш
        args.Message.Text = RegexLowerSh().Replace(args.Message.Text, "шшш");
        // ШШШ
        args.Message.Text = RegexUpperSh().Replace(args.Message.Text, "ШШШ");
        // щщщ
        args.Message.Text = RegexLowerShch().Replace(args.Message.Text, "щщщ");
        // ЩЩЩ
        args.Message.Text = RegexUpperShch().Replace(args.Message.Text, "ЩЩЩ");
    }
}
