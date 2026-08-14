using Content.Server._Goobstation.Religion.AlternatePrayable;
using Content.Shared._Starlight.Vampire.Components;
using Content.Shared.Damage.Systems;
using Content.Shared.Examine;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Server._Goobstation.Religion.HealNearOnPray;

/// <summary>
/// Ported from Goob-Station's Religion system, trimmed down to drop the Shitmed-specific
/// targetPart/splitDamage/silicon/spectral checks this fork doesn't have, and adapted to check
/// UnholyComponent directly instead of raising Goob's DamageUnholyEvent relay (this fork's Bible
/// already does the same direct check - see Content.Server.Bible.BibleSystem).
/// </summary>
public sealed class HealNearOnPraySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ExamineSystemShared _occlusion = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HealNearOnPrayComponent, AlternatePrayEvent>(OnPray);
    }

    private void OnPray(EntityUid uid, HealNearOnPrayComponent comp, ref AlternatePrayEvent args)
    {
        var nearby = _lookup.GetEntitiesInRange<MobStateComponent>(Transform(uid).Coordinates, comp.Range);

        foreach (var entity in nearby)
        {
            if (_mobState.IsDead(entity) || !_occlusion.InRangeUnOccluded(uid, entity, comp.Range))
                continue;

            if (HasComp<UnholyComponent>(entity))
            {
                _damageable.TryChangeDamage(entity.Owner, comp.Damage, true, origin: uid);
                _audio.PlayPvs(comp.SizzleSoundPath, entity.Owner);
            }
            else
            {
                _damageable.TryChangeDamage(entity.Owner, comp.Healing, true, origin: uid);
                Spawn(comp.HealEffect, Transform(entity.Owner).Coordinates);
            }
        }

        _audio.PlayPvs(comp.HealSoundPath, uid);
    }
}
