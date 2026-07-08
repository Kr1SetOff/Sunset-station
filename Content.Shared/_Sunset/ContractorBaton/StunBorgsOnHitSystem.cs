using Content.Shared.Silicons.Borgs.Components;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Sunset.ContractorBaton;

/// <summary>
/// Ported from Goobstation/Reserve-Station: applies <see cref="StunBorgsOnHitComponent"/>'s paralyze
/// to any borg struck in melee (used by ContractorBaton).
/// </summary>
public sealed class StunBorgsOnHitSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StunBorgsOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<StunBorgsOnHitComponent> ent, ref MeleeHitEvent args)
    {
        foreach (var hit in args.HitEntities)
        {
            if (HasComp<BorgChassisComponent>(hit))
                _stun.TryAddParalyzeDuration(hit, ent.Comp.ParalyzeDuration);
        }
    }
}
