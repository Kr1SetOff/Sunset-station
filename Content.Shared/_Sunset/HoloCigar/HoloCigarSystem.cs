using Content.Shared._Starlight.Weapons.DualWield;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory.Events;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Smoking;
using Content.Shared.Verbs;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Shared._Sunset.HoloCigar;

/// <summary>
/// <see cref="HoloCigarComponent"/> - toggling, music, and the "fire both dual-wielded guns
/// together" gunslinger bonus while lit and worn.
/// </summary>
public sealed class HoloCigarSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedDualWieldSystem _dualWield = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedItemSystem _item = default!;
    [Dependency] private readonly INetManager _net = default!;

    private const string LitPrefix = "lit";
    private const string UnlitPrefix = "unlit";
    private const string MaskSlot = "mask";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HoloCigarComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<HoloCigarComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<HoloCigarWearerComponent, MobStateChangedEvent>(OnWearerMobStateChanged);
        SubscribeLocalEvent<HoloCigarWearerComponent, ComponentShutdown>(OnWearerShutdown);

        SubscribeLocalEvent<GunComponent, GunShotEvent>(OnAnyGunShot);
    }

    private void OnGetVerbs(Entity<HoloCigarComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var wearer = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString(ent.Comp.Lit ? "holo-cigar-verb-extinguish" : "holo-cigar-verb-light"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/light.svg.192dpi.png")),
            Act = () => ToggleLit(ent, wearer),
        });
    }

    private void ToggleLit(Entity<HoloCigarComponent> ent, EntityUid wearer)
    {
        ent.Comp.Lit = !ent.Comp.Lit;

        var state = ent.Comp.Lit ? SmokableState.Lit : SmokableState.Unlit;
        var prefix = ent.Comp.Lit ? LitPrefix : UnlitPrefix;
        _appearance.SetData(ent.Owner, SmokingVisuals.Smoking, state);
        _clothing.SetEquippedPrefix(ent.Owner, prefix);
        _item.SetHeldPrefix(ent.Owner, prefix);

        if (ent.Comp.Lit)
        {
            var wearerComp = EnsureComp<HoloCigarWearerComponent>(wearer);
            wearerComp.HoloCigarEntity = ent.Owner;

            if (_net.IsServer)
            {
                var audio = _audio.PlayPvs(ent.Comp.Music, ent.Owner);
                ent.Comp.MusicEntity = audio?.Entity;
            }
        }
        else
        {
            RemComp<HoloCigarWearerComponent>(wearer);
            _audio.Stop(ent.Comp.MusicEntity);
            ent.Comp.MusicEntity = null;
        }
    }

    private void OnUnequipped(Entity<HoloCigarComponent> ent, ref GotUnequippedEvent args)
    {
        if (args.Slot != MaskSlot || !ent.Comp.Lit)
            return;

        ent.Comp.Lit = false;
        RemComp<HoloCigarWearerComponent>(args.Equipee);
        _audio.Stop(ent.Comp.MusicEntity);
        ent.Comp.MusicEntity = null;
    }

    private void OnWearerMobStateChanged(Entity<HoloCigarWearerComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead || !TryComp<HoloCigarComponent>(ent.Comp.HoloCigarEntity, out var cigar))
            return;

        _audio.Stop(cigar.MusicEntity);
        if (_net.IsServer)
            _audio.PlayPvs(cigar.DeathAudio, ent.Owner);
    }

    private void OnWearerShutdown(Entity<HoloCigarWearerComponent> ent, ref ComponentShutdown args)
    {
        if (TryComp<HoloCigarComponent>(ent.Comp.HoloCigarEntity, out var cigar))
            _audio.Stop(cigar.MusicEntity);
    }

    private void OnAnyGunShot(Entity<GunComponent> gun, ref GunShotEvent args)
    {
        if (!TryComp<HoloCigarWearerComponent>(args.User, out var wearer) || wearer.Firing)
            return;

        // No need for the native dual-wield toggle here - wearing a lit holo-cigar with a gun in
        // each hand is enough on its own to make firing one also fire the other.
        if (!_dualWield.TryGetBothGuns(args.User, out var gun1, out var gun2))
            return;

        EntityUid otherGun;
        if (gun.Owner == gun1)
            otherGun = gun2;
        else if (gun.Owner == gun2)
            otherGun = gun1;
        else
            return;

        if (!TryComp<GunComponent>(otherGun, out var otherGunComp)
            || gun.Comp.ShootCoordinates is not { } shootCoordinates)
        {
            return;
        }

        wearer.Firing = true;
        _gun.AttemptShoot(args.User, (otherGun, otherGunComp), shootCoordinates, gun.Comp.Target);
        wearer.Firing = false;
    }
}
