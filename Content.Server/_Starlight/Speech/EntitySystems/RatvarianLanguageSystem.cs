using System.Text;
using System.Text.RegularExpressions;
using Content.Shared.Speech;
using Content.Shared.Speech.Components;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Robust.Shared.Prototypes;

namespace Content.Server._Starlight.Speech.EntitySystems;

public sealed partial class RatvarianLanguageSystem : SharedRatvarianLanguageSystem
{
    [Dependency] private StatusEffectsSystem _statusEffects = default!;

    private static readonly ProtoId<StatusEffectPrototype> _ratvarianKey = "RatvarianLanguage";

    // 🌇Sunset🌇 - the original rules (and the ROT13 letter cipher) were English-morpheme/ASCII
    // specific and a total no-op on Cyrillic text. Reworked with the same "constructed ancient
    // language" feel using Russian's own common function words/clusters and a ROT16 Cyrillic
    // cipher (а-я spans exactly 32 code points, so 32/2=16 keeps the rotation self-inverse):
    /*
     * Any time the word "и" (and) occurs, it's linked to its neighbours by hyphens: "меч-и-щит"
     * Any time the word "от" occurs, it's linked to the following word by a hyphen: "от-этого"
     * Any time the word "то" occurs, it's linked to the following word by a hyphen: "то-время"
     * Any time the word "мой/моя/моё/мои" occurs, it's linked to the following word by a hyphen
     * Wherever the cluster "ст" is followed by a vowel, a grave is inserted after it: "с`тол"
     * Ratvarian proper nouns are not translated: Ратвар, Незбере, Севтук, Нзкрентр, Инат-нек
     */

    [GeneratedRegex(@"ст(?=[аеёиоуыэюя])", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex STPattern();
    [GeneratedRegex(@"\b(\s)(и)(\s)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex IPattern();
    [GeneratedRegex(@"(от|то)\s", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex OTTOPattern();
    [GeneratedRegex(@"(мо[йяёи])\s", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex MOYPattern();
    [GeneratedRegex(@"(ратвар)|(незбере)|(севтук)|(нзкрентр)|(инат-нек)", RegexOptions.IgnoreCase | RegexOptions.Compiled)]
    private static partial Regex ProperNouns();

    public override void Initialize() =>
        // Activate before other modifications so translation works properly
        SubscribeLocalEvent<RatvarianLanguageComponent, AccentGetEvent>(OnAccent, before: new[] { typeof(SharedSlurredSystem), typeof(SharedStutteringSystem) });

    public override void DoRatvarian(EntityUid uid, TimeSpan time, bool refresh, StatusEffectsComponent? status = null)
    {
        if (!Resolve(uid, ref status, false))
            return;

        _statusEffects.TryAddStatusEffect<RatvarianLanguageComponent>(uid, _ratvarianKey, time, refresh, status);
    }

    private void OnAccent(EntityUid uid, RatvarianLanguageComponent component, AccentGetEvent args)
        => args.Message.Text = Translate(args.Message.Text);

    private static string Translate(string message)
    {
        var ruleTranslation = message;
        var finalMessage = new StringBuilder();
        var newWord = new StringBuilder();

        ruleTranslation = STPattern().Replace(ruleTranslation, "$&`");
        ruleTranslation = OTTOPattern().Replace(ruleTranslation, "$1-");
        ruleTranslation = MOYPattern().Replace(ruleTranslation, "$1-");
        ruleTranslation = IPattern().Replace(ruleTranslation, "-$2-");

        var temp = ruleTranslation.Split(' ');

        foreach (var word in temp)
        {
            newWord.Clear();

            if (ProperNouns().IsMatch(word))
                newWord.Append(word);

            else
            {
                for (int i = 0; i < word.Length; i++)
                {
                    var letter = word[i];

                    if (letter is >= 'а' and <= 'я') // а-я
                    {
                        var idx = letter - 'а';
                        var rotIdx = (idx + 16) % 32;
                        newWord.Append((char) ('а' + rotIdx));
                    }
                    else if (letter is >= 'А' and <= 'Я') // А-Я
                    {
                        var idx = letter - 'А';
                        var rotIdx = (idx + 16) % 32;
                        newWord.Append((char) ('А' + rotIdx));
                    }
                    else
                    {
                        newWord.Append(word[i]);
                    }
                }
            }
            finalMessage.Append(newWord + " ");
        }
        return finalMessage.ToString().Trim();
    }
}
