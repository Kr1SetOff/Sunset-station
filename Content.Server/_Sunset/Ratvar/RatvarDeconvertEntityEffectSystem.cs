using Content.Shared._Sunset.Ratvar;
using Content.Shared.EntityEffects;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - see RatvarDeconvert. Gated by MinScale (the "40+ units" splash threshold, set on the
/// entry in Reagents/medicine.yml) and a 150s per-servant cooldown here so a lingering Touch reaction
/// (soaked clothing, a chem mist) can't spam the deconversion.
/// </summary>
public sealed partial class RatvarDeconvertEntityEffectSystem : EntityEffectSystem<RatvarServantComponent, RatvarDeconvert>
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly RatvarServantSystem _ratvarServant = default!;

    private static readonly TimeSpan Cooldown = TimeSpan.FromSeconds(150);

    protected override void Effect(Entity<RatvarServantComponent> entity, ref EntityEffectEvent<RatvarDeconvert> args)
    {
        var now = _timing.CurTime;
        if (now < entity.Comp.NextDeconvertAttempt)
            return;

        entity.Comp.NextDeconvertAttempt = now + Cooldown;
        _ratvarServant.DeconvertServant(entity.Owner);
    }
}
