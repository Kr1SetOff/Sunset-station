using Content.Shared.EntityEffects;
using Content.Shared.StatusEffectNew;

namespace Content.Shared.Drugs;

/// <summary>
/// 🌇Sunset🌇 - see <see cref="AddHallucinationTheme"/>.
/// </summary>
public sealed partial class AddHallucinationThemeEntityEffectSystem : EntityEffectSystem<MetaDataComponent, AddHallucinationTheme>
{
    [Dependency] private readonly StatusEffectsSystem _status = default!;

    protected override void Effect(Entity<MetaDataComponent> entity, ref EntityEffectEvent<AddHallucinationTheme> args)
    {
        if (!_status.TryGetStatusEffect(entity, "StatusEffectSeeingRainbow", out var statusEffect))
            return;

        var theme = EnsureComp<HallucinationMobsComponent>(statusEffect.Value);
        theme.Mob = args.Effect.Mob;
        Dirty(statusEffect.Value, theme);
    }
}
