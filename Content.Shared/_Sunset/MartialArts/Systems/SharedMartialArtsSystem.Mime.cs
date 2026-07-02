using Content.Shared.Damage;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Speech.Muting;
using Content.Shared._Sunset.MartialArts.Components;

namespace Content.Shared._Sunset.MartialArts.Systems;

public sealed partial class SharedMartialArtsSystem
{
    private void InitializeMime()
    {
    }

    /// <summary>
    /// Traces out an invisible wall and shoves the target into it - no real wall, just a stagger and a
    /// dented sense of dignity.
    /// </summary>
    private void MimeInvisibleWall(EntityUid user, EntityUid target)
    {
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(2), force: true);
        _stamina.TakeStaminaDamage(target, 25f, source: user);
    }

    /// <summary>
    /// A scream with no sound to it - the target loses their voice for a while.
    /// </summary>
    private void MimeSilentScream(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 8 } } }, origin: user);

        EnsureComp<MutedComponent>(target);
        var tempMute = EnsureComp<TemporaryMuteComponent>(target);
        tempMute.ExpiresAt = _timing.CurTime + TimeSpan.FromSeconds(8);
    }

    /// <summary>
    /// Mimes an invisible box around the target and slams the lid - only really works on someone you've
    /// already got a hold of.
    /// </summary>
    private void MimeBoxTrap(EntityUid user, EntityUid target)
    {
        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, user);

        _stun.TryKnockdown(target, TimeSpan.FromSeconds(3), force: true);
        _stamina.TakeStaminaDamage(target, 30f, source: user);
    }

    /// <summary>
    /// A big, theatrical, exaggerated slap - the payoff move once the target is already reeling.
    /// </summary>
    private void MimeExaggeratedSlap(EntityUid user, EntityUid target)
    {
        _damageable.TryChangeDamage(target, new DamageSpecifier { DamageDict = new() { { "Blunt", 15 } } }, origin: user);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(1), force: true);
    }
}
