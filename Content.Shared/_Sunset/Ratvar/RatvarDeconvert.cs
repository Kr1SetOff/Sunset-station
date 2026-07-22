using Content.Shared.EntityEffects;

namespace Content.Shared._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - holy water counterplay (RU wiki: a big enough splash breaks a Servant's conversion).
/// Piggybacks on Holywater's existing "Unholy" reactiveEffects entry (see Reagents/medicine.yml) instead
/// of a bespoke reagent - the RatvarServantComponent type parameter on
/// RatvarDeconvertEntityEffectSystem (Content.Server) makes this a no-op for every other entity reactive
/// to Unholy (vampires, etc), and MinScale on the entry gates the "40+ units" threshold without any
/// manual scale check.
/// </summary>
public sealed partial class RatvarDeconvert : EntityEffectBase<RatvarDeconvert>;
