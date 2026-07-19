using Content.Server.Speech.Components;
using Content.Shared._Starlight.Speech;
using Content.Shared.Speech;
using Content.Shared.StatusEffectNew;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed class OwOAccentSystem : EntitySystem
{
    // 🌇Sunset🌇 - the original was an English word dictionary + Latin r/l->w swap, both no-ops on
    // Cyrillic text.
    private static readonly IReadOnlyDictionary<string, string> _specialWords = new Dictionary<string, string>()
    {
        { "ты", "тывы" },
        { "привет", "мяу" },
        { "любовь", "любофф" },
        { "пожалуйста", "пожаааалста" },
        { "еда", "нямка" },
        { "милый", "кьют" },
        { "сейчас", "мяусчас" },
        { "смотри", "смотвики" },
        { "маленький", "малюсь" },
    };

    public override void Initialize()
    {
        SubscribeLocalEvent<OwOAccentComponent, AccentGetEvent>(OnAccent);
        SubscribeLocalEvent<OwOAccentComponent, StatusEffectRelayedEvent<AccentGetEvent>>(OnAccentRelayed);
    }

    public SpeechMessage Accentuate(SpeechMessage message)
    {
        foreach (var (word, repl) in _specialWords)
        {
            message.Text = message.Text.Replace(word, repl);
            message.Tts = (message.Tts ?? message.Text).Replace(word, repl);
        }
        // р/л -> в is the Russian analogue of the English r/l -> w "baby talk" mispronunciation
        // (rhotacism substituting в for р is a real, recognizable childish-speech pattern).
        message.Text = message.Text
            .Replace("р", "в").Replace("Р", "В")
            .Replace("л", "в").Replace("Л", "В");

        return message;
    }

    private void OnAccent(Entity<OwOAccentComponent> entity, ref AccentGetEvent args)
        => args.Message = Accentuate(args.Message);

    private void OnAccentRelayed(Entity<OwOAccentComponent> entity, ref StatusEffectRelayedEvent<AccentGetEvent> args)
        => args.Args.Message = Accentuate(args.Args.Message);
}
