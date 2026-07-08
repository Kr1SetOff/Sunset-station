using Content.Shared.Actions;
using Content.Shared.Damage.Systems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared._Sunset.Sandevistan;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Events;
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Shared._Sunset.HotlineMiami;

/// <summary>
/// Wearing the full Hotline Miami zebra costume (mask + jacket + tank top, each tagged with
/// ZebraCostumePieceComponent) grants a triggered "roll": for RollDuration, the wearer takes no
/// damage at all (a full dodge-roll invulnerability window, not just to gunfire - simpler and more
/// robust than trying to distinguish bullet damage specifically) and moves faster, leaving a trail of
/// fading afterimages (reusing SandevistanAfterimageComponent's client-side renderer verbatim - there's
/// no dedicated roll sprite, and this gives an effective-looking dash trail for free). Touching a
/// Mannequin ("doll") while rolling knocks it over.
/// </summary>
public sealed class ZebraRollSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _speed = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private static readonly SoundSpecifier RollSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ZebraCostumePieceComponent, GotEquippedEvent>(OnPieceEquipped);
        SubscribeLocalEvent<ZebraCostumePieceComponent, GotUnequippedEvent>(OnPieceUnequipped);

        SubscribeLocalEvent<ZebraRollComponent, ZebraRollActionEvent>(OnRollAction);
        SubscribeLocalEvent<ZebraRollComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        SubscribeLocalEvent<ZebraRollComponent, BeforeDamageChangedEvent>(OnBeforeDamage);
        SubscribeLocalEvent<ZebraRollComponent, StartCollideEvent>(OnRollCollide);
    }

    private void OnPieceEquipped(Entity<ZebraCostumePieceComponent> ent, ref GotEquippedEvent args)
    {
        RefreshZebraSet(args.Equipee);
    }

    private void OnPieceUnequipped(Entity<ZebraCostumePieceComponent> ent, ref GotUnequippedEvent args)
    {
        RefreshZebraSet(args.Equipee);
    }

    private void RefreshZebraSet(EntityUid wearer)
    {
        var hasFullSet =
            _inventory.TryGetSlotEntity(wearer, "mask", out var mask) && HasComp<ZebraCostumePieceComponent>(mask) &&
            _inventory.TryGetSlotEntity(wearer, "outerClothing", out var jacket) && HasComp<ZebraCostumePieceComponent>(jacket) &&
            _inventory.TryGetSlotEntity(wearer, "jumpsuit", out var uniform) && HasComp<ZebraCostumePieceComponent>(uniform);

        if (hasFullSet)
        {
            var roll = EnsureComp<ZebraRollComponent>(wearer);
            _actions.AddAction(wearer, ref roll.RollAction, "ActionHotlineZebraRoll");
            return;
        }

        if (!TryComp<ZebraRollComponent>(wearer, out var existing))
            return;

        _actions.RemoveAction(existing.RollAction);

        if (existing.Rolling)
            _speed.RefreshMovementSpeedModifiers(wearer);

        RemComp<ZebraRollComponent>(wearer);
    }

    private void OnRollAction(Entity<ZebraRollComponent> ent, ref ZebraRollActionEvent args)
    {
        if (args.Handled || ent.Comp.Rolling)
            return;

        args.Handled = true;

        ent.Comp.Rolling = true;
        ent.Comp.RollEndTime = _timing.CurTime + ent.Comp.RollDuration;
        ent.Comp.NextAfterimageTime = _timing.CurTime;
        Dirty(ent);

        _speed.RefreshMovementSpeedModifiers(ent.Owner);
        _popup.PopupClient(Loc.GetString("hotline-zebra-roll-start"), ent.Owner, ent.Owner);
        _audio.PlayPredicted(RollSound, ent.Owner, ent.Owner);
    }

    private void OnRefreshSpeed(Entity<ZebraRollComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.Rolling)
            args.ModifySpeed(ent.Comp.RollSpeedMultiplier, ent.Comp.RollSpeedMultiplier);
    }

    private void OnBeforeDamage(Entity<ZebraRollComponent> ent, ref BeforeDamageChangedEvent args)
    {
        if (ent.Comp.Rolling)
            args.Cancelled = true;
    }

    private void OnRollCollide(Entity<ZebraRollComponent> ent, ref StartCollideEvent args)
    {
        if (!ent.Comp.Rolling || !args.OtherFixture.Hard)
            return;

        if (MetaData(args.OtherEntity).EntityPrototype?.ID != "Mannequin")
            return;

        var xform = Transform(args.OtherEntity);
        _transform.SetLocalRotation(args.OtherEntity, xform.LocalRotation + Angle.FromDegrees(90));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<ZebraRollComponent>();
        while (query.MoveNext(out var uid, out var roll))
        {
            if (!roll.Rolling)
                continue;

            if (curTime >= roll.RollEndTime)
            {
                roll.Rolling = false;
                Dirty(uid, roll);
                _speed.RefreshMovementSpeedModifiers(uid);
                continue;
            }

            if (curTime < roll.NextAfterimageTime)
                continue;

            SpawnAfterimage(uid, roll);
            roll.NextAfterimageTime = curTime + TimeSpan.FromSeconds(roll.AfterimageInterval);
        }
    }

    private void SpawnAfterimage(EntityUid uid, ZebraRollComponent roll)
    {
        var xform = Transform(uid);
        var afterimage = Spawn(null, xform.Coordinates);

        var afterimageComp = EnsureComp<SandevistanAfterimageComponent>(afterimage);
        afterimageComp.SourceEntity = uid;
        afterimageComp.Hue = roll.ColorAccumulator % 100f / 100f;
        afterimageComp.DirectionOverride = xform.LocalRotation.GetCardinalDir();
        Dirty(afterimage, afterimageComp);

        roll.ColorAccumulator++;

        var despawn = EnsureComp<TimedDespawnComponent>(afterimage);
        despawn.Lifetime = 1f;
    }
}
