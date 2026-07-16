using Content.Server.Atmos.EntitySystems;
using Content.Shared._Sunset.Chemistry.Components;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.Chemistry;

/// <summary>
/// 🌇Sunset🌇 - runtime half of the Zapalm reagent: every pulse, anything flammable within the
/// star pattern (two tiles out in each cardinal direction) around the carrier catches fire, and
/// the carrier themselves slowly cooks - ~50 burn damage over the full 15 minutes.
/// </summary>
public sealed class ZapalmAuraSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly FlammableSystem _flammable = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly TimeSpan PulseInterval = TimeSpan.FromSeconds(1);

    // 50 damage over 15 minutes = ~0.056 per one-second pulse.
    private const float SelfBurnPerPulse = 50f / 900f;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<ZapalmAuraComponent>();
        while (query.MoveNext(out var uid, out var aura))
        {
            if (now >= aura.EndTime)
            {
                RemComp<ZapalmAuraComponent>(uid);
                continue;
            }

            if (now < aura.NextPulse)
                continue;

            aura.NextPulse = now + PulseInterval;
            Pulse(uid);
        }
    }

    private void Pulse(EntityUid uid)
    {
        // The carrier slow-cooks in their own flames.
        _damageable.TryChangeDamage(uid, new DamageSpecifier { DamageDict = { { "Heat", SelfBurnPerPulse } } }, true, false);

        // Ignite everything flammable in a star/plus pattern: two tiles out along each cardinal
        // axis (not diagonals), centered on the carrier.
        var ourPos = _transform.GetWorldPosition(uid);
        foreach (var target in _lookup.GetEntitiesInRange<FlammableComponent>(_transform.GetMapCoordinates(uid), 2.5f))
        {
            if (target.Owner == uid)
                continue;

            var offset = _transform.GetWorldPosition(target) - ourPos;
            var dx = MathF.Abs(offset.X);
            var dy = MathF.Abs(offset.Y);

            // Cross shape: close to one axis, within two tiles along the other.
            if (!(dx <= 0.6f && dy <= 2.5f) && !(dy <= 0.6f && dx <= 2.5f))
                continue;

            _flammable.AdjustFireStacks(target, 1f, target.Comp, ignite: true);
        }
    }
}
