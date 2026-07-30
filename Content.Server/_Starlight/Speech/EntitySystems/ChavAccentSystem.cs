using Content.Server._Starlight.Speech.Components;
using Content.Server.Speech.EntitySystems;
using Content.Shared.Speech;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class ChavAccentSystem : EntitySystem
{
    [Dependency] private ReplacementAccentSystem _replacement = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChavAccentComponent, AccentGetEvent>(OnAccent);
    }

    private void OnAccent(EntityUid uid, ChavAccentComponent component, AccentGetEvent args)
    {
        args.Message = _replacement.ApplyReplacements(args.Message, "chav");

        // 🌇Sunset🌇 - "th"->"ff" was an English-only spelling gag (Cockney "faf/muvva"), which is a
        // silent no-op on Cyrillic text. Swapped for the equivalent common reduced-speech
        // contractions in colloquial Russian.
        args.Message.Text = args.Message.Text
            .Replace("вообще", "ваще")
            .Replace("Вообще", "Ваще")
            .Replace("конечно", "канеш")
            .Replace("Конечно", "Канеш")
            .Replace("сейчас", "щас")
            .Replace("Сейчас", "Щас");
    }
}
