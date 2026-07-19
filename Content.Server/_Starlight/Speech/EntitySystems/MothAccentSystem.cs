using System.Text.RegularExpressions;
using Content.Server.Speech.Components;
using Content.Shared.Speech;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class MothAccentSystem : EntitySystem
{
    // 🌇Sunset🌇 - matched Latin z, a no-op on Cyrillic. з is Russian's buzzing consonant.
    [GeneratedRegex("з{1,3}", RegexOptions.IgnoreCase)]
    private static partial Regex RegexBuzz();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MothAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, MothAccentComponent component, AccentGetEvent args) =>
        // жжжз - extend з sounds
        args.Message.Text = RegexBuzz().Replace(args.Message.Text, m =>
            char.IsUpper(m.Value[0]) ? "ЗЗЗ" : "ззз");
}
