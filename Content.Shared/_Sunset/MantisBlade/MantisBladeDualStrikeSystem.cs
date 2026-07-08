using System.Linq;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._Sunset.MantisBlade;

/// <summary>
/// If both cyber-arms' mantis blades are deployed (one in each hand), striking with one also
/// strikes with the other - same "fire from both" idea as the holo-cigar's dual-wield gunfire
/// (Content.Shared._Sunset.HoloCigar), just for melee. Identifies the blades by prototype id
/// (CyberMantisBlade, from _Starlight/Entities/Objects/Weapons/Melee/cyberlimb.yml) rather than a
/// marker component, so nothing outside _Sunset needs editing.
/// </summary>
public sealed class MantisBladeDualStrikeSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedMeleeWeaponSystem _melee = default!;

    private const string MantisBladeProtoId = "CyberMantisBlade";

    private readonly HashSet<EntityUid> _striking = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeAllEvent<LightAttackEvent>(OnLightAttack);
    }

    private bool IsMantisBlade(EntityUid uid) =>
        Comp<MetaDataComponent>(uid).EntityPrototype?.ID == MantisBladeProtoId;

    private void OnLightAttack(LightAttackEvent msg, EntitySessionEventArgs args)
    {
        if (args.SenderSession.AttachedEntity is not { } user || !_striking.Add(user))
            return;

        try
        {
            var weaponUid = GetEntity(msg.Weapon);
            if (!IsMantisBlade(weaponUid) || msg.Target is not { } netTarget)
                return;

            var target = GetEntity(netTarget);

            var otherBlade = _hands.EnumerateHeld(user)
                .FirstOrDefault(held => held != weaponUid && IsMantisBlade(held));

            if (otherBlade == default || !TryComp<MeleeWeaponComponent>(otherBlade, out var otherWeapon))
                return;

            _melee.AttemptLightAttack(user, otherBlade, otherWeapon, target);
        }
        finally
        {
            _striking.Remove(user);
        }
    }
}
