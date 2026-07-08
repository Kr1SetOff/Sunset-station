using System.Linq;
using Content.Server._Starlight.Medical.Limbs;
using Content.Shared._Starlight.Medical.Body.Part;
using Content.Shared._Sunset.Autosurgeon;
using Content.Shared._Sunset.Sandevistan;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Humanoid;
using Content.Shared.Interaction.Events;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Log;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Server._Sunset.Autosurgeon;

/// <summary>
/// <see cref="AutosurgeonComponent"/> - handles the self-use DoAfter and swaps a body part or organ.
/// Server-only: replacing a body part needs <see cref="LimbSystem.AttachLimb"/> (not just the bare
/// <see cref="SharedBodySystem.AttachPart"/>) so the new limb's nested hand actually gets registered
/// on the wearer's HandsComponent - the same thing this fork's other cyberlimb-attach code paths do.
/// </summary>
public sealed class AutosurgeonSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly LimbSystem _limb = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _moveSpeed = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private ISawmill _sawmill = default!;

    public override void Initialize()
    {
        base.Initialize();

        _sawmill = Logger.GetSawmill("autosurgeon");

        SubscribeLocalEvent<AutosurgeonComponent, UseInHandEvent>(OnUse);
        SubscribeLocalEvent<AutosurgeonComponent, AutosurgeonDoAfterEvent>(OnDoAfter);
    }

    private void OnUse(Entity<AutosurgeonComponent> ent, ref UseInHandEvent args)
    {
        if (ent.Comp.Used || args.Handled)
            return;

        args.Handled = true;

        _doAfter.TryStartDoAfter(new DoAfterArgs(
            EntityManager,
            args.User,
            ent.Comp.DoAfterTime,
            new AutosurgeonDoAfterEvent(),
            ent.Owner,
            args.User,
            ent.Owner)
        {
            BreakOnMove = true,
            DistanceThreshold = 0.5f,
        });

        // PlayPredicted excludes "user" from hearing the sound, assuming the client already played
        // it locally via its own prediction - but this system is Content.Server-only (needed for
        // LimbSystem.AttachLimb, see ReplaceBodyPart), so that assumption is false and the person
        // using the autosurgeon on themselves would never hear it start. PlayPvs broadcasts to
        // everyone, including them.
        _audio.PlayPvs(ent.Comp.Sound, ent.Owner);
    }

    private void OnDoAfter(Entity<AutosurgeonComponent> ent, ref AutosurgeonDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled || ent.Comp.Used || args.Args.Target is not { } target)
            return;

        var success = false;

        if (ent.Comp.NewPartProto is { } newPartProto && ent.Comp.TargetPartType is { } targetPartType)
            success = ReplaceBodyPart(target, args.Args.User, targetPartType, ent.Comp.TargetSymmetry, newPartProto);
        else if (ent.Comp.NewOrganProto is { } newOrganProto && ent.Comp.TargetOrganSlot is { } targetOrganSlot)
            success = ReplaceOrgan(target, args.Args.User, targetOrganSlot, newOrganProto);

        args.Handled = true;

        if (!success)
        {
            _popup.PopupClient(Loc.GetString(ent.Comp.FailurePopup), target, target);
            return;
        }

        if (ent.Comp.MoveSpeedMultiplier is { } speedMultiplier)
        {
            var moveMod = EnsureComp<MovementSpeedModifierComponent>(target);
            _moveSpeed.ChangeBaseSpeed(target,
                moveMod.BaseWalkSpeed * speedMultiplier,
                moveMod.BaseSprintSpeed * speedMultiplier,
                moveMod.Acceleration,
                moveMod);
        }

        ent.Comp.Used = true;
        _popup.PopupClient(Loc.GetString(ent.Comp.SuccessPopup), target, target);
        QueueDel(ent.Owner);
    }

    private bool ReplaceBodyPart(EntityUid target, EntityUid user, BodyPartType partType, BodyPartSymmetry symmetry, EntProtoId newPartProto)
    {
        var oldPart = _body.GetBodyChildrenOfType(target, partType)
            .FirstOrDefault(p => p.Component.Symmetry == symmetry)
            .Id;

        if (!oldPart.Valid)
        {
            _sawmill.Warning($"[{newPartProto}] no existing {symmetry} {partType} found on {ToPrettyString(target)}");
            return false;
        }

        var parentAndSlot = _body.GetParentPartAndSlotOrNull(oldPart);
        if (parentAndSlot is not { } parentSlot)
        {
            _sawmill.Warning($"[{newPartProto}] could not resolve parent/slot for old part {ToPrettyString(oldPart)}");
            return false;
        }

        if (!TryComp<HumanoidAppearanceComponent>(target, out var humanoid))
        {
            _sawmill.Warning($"[{newPartProto}] {ToPrettyString(target)} has no HumanoidAppearanceComponent, can't attach limb");
            return false;
        }

        if (!TryComp<BodyPartComponent>(parentSlot.Parent, out var parentPart))
        {
            _sawmill.Warning($"[{newPartProto}] parent part {ToPrettyString(parentSlot.Parent)} has no BodyPartComponent");
            return false;
        }

        var coords = Transform(target).Coordinates;

        if (_container.TryGetContainer(parentSlot.Parent, SharedBodySystem.GetPartSlotContainerId(parentSlot.Slot), out var container))
        {
            var removed = _container.Remove(oldPart, container);
            _sawmill.Info($"[{newPartProto}] removed old part {ToPrettyString(oldPart)} from slot '{parentSlot.Slot}' on {ToPrettyString(parentSlot.Parent)}: {removed}");
        }
        else
        {
            _sawmill.Warning($"[{newPartProto}] could not find container for slot '{parentSlot.Slot}' on {ToPrettyString(parentSlot.Parent)}");
            return false;
        }

        DropRemovedPart(oldPart, user, coords);

        var newPart = Spawn(newPartProto, coords);
        var newPartComp = Comp<BodyPartComponent>(newPart);
        var attached = _limb.AttachLimb((target, humanoid), parentSlot.Slot, (parentSlot.Parent, parentPart), (newPart, newPartComp));
        _sawmill.Info($"[{newPartProto}] attached new part {ToPrettyString(newPart)} to slot '{parentSlot.Slot}' on {ToPrettyString(parentSlot.Parent)}: {attached}");
        return attached;
    }

    private bool ReplaceOrgan(EntityUid target, EntityUid user, string organSlot, EntProtoId newOrganProto)
    {
        var torso = _body.GetBodyChildrenOfType(target, BodyPartType.Torso).FirstOrDefault().Id;
        if (!torso.Valid)
        {
            _sawmill.Warning($"[{newOrganProto}] no torso found on {ToPrettyString(target)}");
            return false;
        }

        if (!_container.TryGetContainer(torso, SharedBodySystem.GetOrganContainerId(organSlot), out var container))
        {
            _sawmill.Warning($"[{newOrganProto}] no organ slot '{organSlot}' container found on torso {ToPrettyString(torso)}");
            return false;
        }

        var coords = Transform(target).Coordinates;
        var oldOrgan = container.ContainedEntities.FirstOrDefault();

        if (oldOrgan.Valid)
        {
            if (HasComp<SandevistanHeartComponent>(oldOrgan))
                RemComp<SandevistanUserComponent>(target);

            var removed = _body.RemoveOrgan(oldOrgan);
            _sawmill.Info($"[{newOrganProto}] removed old organ {ToPrettyString(oldOrgan)} from slot '{organSlot}': {removed}");
            DropRemovedPart(oldOrgan, user, coords);
        }
        else
        {
            _sawmill.Warning($"[{newOrganProto}] no existing organ found in slot '{organSlot}' on torso {ToPrettyString(torso)}");
        }

        var newOrgan = Spawn(newOrganProto, coords);
        var inserted = _body.InsertOrgan(torso, newOrgan, organSlot);
        _sawmill.Info($"[{newOrganProto}] inserted new organ {ToPrettyString(newOrgan)} into slot '{organSlot}': {inserted}");

        if (inserted && TryComp<SandevistanHeartComponent>(newOrgan, out var sandevistanHeart))
            GrantSandevistan(target, sandevistanHeart);

        return inserted;
    }

    private void GrantSandevistan(EntityUid target, SandevistanHeartComponent heart)
    {
        var sandevistan = EnsureComp<SandevistanUserComponent>(target);
        sandevistan.LoadPerActiveSecond = heart.LoadPerActiveSecond;
        sandevistan.LoadPerInactiveSecond = heart.LoadPerInactiveSecond;
        sandevistan.Thresholds = new(heart.Thresholds);
        sandevistan.MovementSpeedModifier = heart.MovementSpeedModifier;
        sandevistan.AttackSpeedModifier = heart.AttackSpeedModifier;
        sandevistan.SlowfieldEnabled = heart.SlowfieldEnabled;
        Dirty(target, sandevistan);
    }

    private void DropRemovedPart(EntityUid part, EntityUid user, EntityCoordinates coords)
    {
        _transform.SetCoordinates(part, coords);
        _hands.TryPickupAnyHand(user, part);
    }
}
