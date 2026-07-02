using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Reflect;
using Content.Shared._Sunset.MartialArts.Components;
using Robust.Shared.Maths;

namespace Content.Shared._Sunset.MartialArts.Systems;

public sealed partial class SharedMartialArtsSystem
{
    private void InitializeSleepingCarp()
    {
    }

    private void OnCarpShotAttempt(ref ShotAttemptedEvent args)
    {
        if (TryComp<SleepingCarpMasteryComponent>(args.User, out var mastery) && mastery.Stage >= 1)
        {
            args.Cancel();
            _popup.PopupClient(Loc.GetString("martial-arts-carp-no-guns"), args.User, args.User);
        }
    }

    /// <summary>
    /// Sleeping Carp's scroll isn't a one-shot grant like the other manuals - it must be re-used 4
    /// times total (each gated by a random cooldown) before it finally teaches the style, matching
    /// Goob Station's exact scroll-training ritual instead of an automatic timer.
    /// </summary>
    private void OnSleepingCarpScrollUsed(Entity<MartialArtManualComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp<MartialArtsKnowledgeComponent>(args.User, out var existing) && existing.Style != MartialArtStyle.None)
        {
            _popup.PopupClient(Loc.GetString("martial-arts-already-known"), args.User, args.User);
            return;
        }

        args.Handled = true;
        var student = EnsureComp<SleepingCarpMasteryComponent>(args.User);

        if (student.Stage > 0 && _timing.CurTime < student.NextStageReady)
        {
            _popup.PopupClient(Loc.GetString("martial-arts-carp-still-training"), args.User, args.User);
            return;
        }

        if (student.Stage >= 3)
        {
            if (!TryGrantMartialArt(args.User, MartialArtStyle.SleepingCarp))
                return;

            GrantCarpReflect(args.User);
            _popup.PopupEntity(Loc.GetString("martial-arts-carp-mastered"), args.User, args.User, PopupType.Large);
            PredictedQueueDel(ent.Owner);
            return;
        }

        AdvanceCarpMastery(args.User, student);
    }

    private void CarpGnashingTeeth(EntityUid user, EntityUid target)
    {
        if (!TryComp<SleepingCarpMasteryComponent>(user, out var mastery))
            return;

        var curTime = _timing.CurTime;
        if (curTime - mastery.LastGnashTime > TimeSpan.FromSeconds(5))
            mastery.ConsecutiveGnashes = 0;

        var damage = 20 + mastery.ConsecutiveGnashes * 5;
        mastery.ConsecutiveGnashes++;
        mastery.LastGnashTime = curTime;
        Dirty(user, mastery);

        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Slash", damage } } }, origin: user);
    }

    private void CarpKneeHaul(EntityUid user, EntityUid target)
    {
        var downed = HasComp<KnockedDownComponent>(target);
        var damage = downed ? 5 : 10;

        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", damage } } }, origin: user);
        _stamina.TakeStaminaDamage(target, downed ? 40f : 60f, source: user);

        if (!downed)
            _stun.TryKnockdown(target, TimeSpan.FromSeconds(6), force: true);
    }

    private void CarpCrashingWaves(EntityUid user, EntityUid target)
    {
        _stamina.TakeStaminaDamage(target, 25f, source: user);
        ThrowAt(user, target, 7f);
    }

    private void AdvanceCarpMastery(EntityUid uid, SleepingCarpMasteryComponent mastery)
    {
        mastery.Stage++;
        mastery.NextStageReady = _timing.CurTime + RandomStageDelay(mastery);
        Dirty(uid, mastery);

        _popup.PopupEntity(Loc.GetString("martial-arts-carp-stage-advance", ("stage", mastery.Stage)), uid, uid, PopupType.Medium);
    }

    private void GrantCarpReflect(EntityUid uid)
    {
        var reflect = EnsureComp<ReflectComponent>(uid);
        reflect.ReflectProb = 1f;
        reflect.Spread = Angle.FromDegrees(60);
        reflect.ReflectingInHands = true;
        Dirty(uid, reflect);
    }

    private TimeSpan RandomStageDelay(SleepingCarpMasteryComponent mastery) =>
        TimeSpan.FromSeconds(_random.NextFloat((float) mastery.StageDelayMin.TotalSeconds, (float) mastery.StageDelayMax.TotalSeconds));
}
