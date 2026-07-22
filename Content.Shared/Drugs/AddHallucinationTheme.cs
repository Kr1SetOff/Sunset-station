using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared.Drugs;

/// <summary>
/// 🌇Sunset🌇 - reagent metabolism effect that tags the reagent's existing "hallucinations" status
/// effect (<c>StatusEffectSeeingRainbow</c>) with which illusion the drug should spawn client-side.
/// Placed alongside a <c>ModifyStatusEffect</c> Add entry in a reagent's metabolism list - deliberately
/// leaves <c>StatusEffectSeeingRainbow</c> itself untouched (same prototype id for every drug) so
/// existing cures that remove it by id (Psicodine, Haloperidol) keep working unchanged.
/// </summary>
public sealed partial class AddHallucinationTheme : EntityEffectBase<AddHallucinationTheme>
{
    [DataField(required: true)]
    public EntProtoId Mob;
}
