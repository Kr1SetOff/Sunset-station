using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared.EntityEffects.Effects;

/// <summary>
/// 🌇Sunset🌇 - teleports the metabolizer to a random safe tile on a random station. Used by
/// the BluespaceLiquid reagent.
/// </summary>
public sealed partial class SunsetRandomTeleport : EntityEffectBase<SunsetRandomTeleport>
{
    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys, ILocalizationManager loc)
        => loc.GetString("entity-effect-guidebook-sunset-random-teleport", ("chance", Probability));
}

/// <summary>
/// 🌇Sunset🌇 - grants BluespacePhaseComponent for a duration: the metabolizer's fixtures go
/// non-hard, letting them walk through walls/doors/anything. Used by BluespaceDistorter.
/// </summary>
public sealed partial class SunsetBluespacePhase : EntityEffectBase<SunsetBluespacePhase>
{
    [DataField]
    public float DurationSeconds = 900f;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys, ILocalizationManager loc)
        => loc.GetString("entity-effect-guidebook-sunset-bluespace-phase", ("chance", Probability), ("minutes", DurationSeconds / 60f));
}

/// <summary>
/// 🌇Sunset🌇 - grants ZapalmAuraComponent for a duration: a star-shaped fire aura that ignites
/// anyone approaching the metabolizer while slowly burning the carrier too. Used by Zapalm.
/// </summary>
public sealed partial class SunsetZapalmAura : EntityEffectBase<SunsetZapalmAura>
{
    [DataField]
    public float DurationSeconds = 900f;

    public override string EntityEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys, ILocalizationManager loc)
        => loc.GetString("entity-effect-guidebook-sunset-zapalm-aura", ("chance", Probability), ("minutes", DurationSeconds / 60f));
}
