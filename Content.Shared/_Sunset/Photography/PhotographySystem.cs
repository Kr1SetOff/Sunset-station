using Content.Shared.Charges.Components;
using Content.Shared.Charges.Systems;
using Content.Shared.EntityTable;
using Content.Shared.Examine;
using Content.Shared.Flash;
using Content.Shared.Flash.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.IdentityManagement;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;
using Robust.Shared.Utility;

namespace Content.Shared._Sunset.Photography;

/// <summary>
/// Handles everything related to photography: taking a picture of an entity you point the camera at,
/// and printing it out when the flash goes off.
/// </summary>
public sealed partial class PhotographySystem : EntitySystem
{
    [Dependency] private ExamineSystemShared _examine = default!;
    [Dependency] private SharedHandsSystem _hands = default!;
    [Dependency] private EntityTableSystem _tables = default!;
    [Dependency] private SharedFlashSystem _flash = default!;
    [Dependency] private SharedChargesSystem _charges = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PhotographComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PictureTakerComponent, AfterFlashedEvent>(OnFlashed);
        SubscribeLocalEvent<PictureTakerComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnExamined(Entity<PhotographComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange)
            return;

        using (args.PushGroup(nameof(PhotographComponent)))
        {
            if (string.IsNullOrEmpty(ent.Comp.NameText))
                args.PushText(Loc.GetString("photograph-name-text-empty"));
            else
                args.PushText(ent.Comp.NameText);
            if (ent.Comp.Description != null)
                // Cloned to avoid ExamineSystem appending a newline to our stored description on every examine.
                args.PushMessage(new FormattedMessage(ent.Comp.Description));
        }
    }

    /// <summary>
    /// Aiming the camera at someone and clicking on them pops the flash and snaps their picture.
    /// This Flash system has no built-in "aimed at a single target" trigger (only AOE use-in-hand
    /// and melee), so the camera gets its own single-target trigger here, going through the same
    /// SharedFlashSystem.Flash() call that use-in-hand/melee flashes use.
    /// </summary>
    private void OnAfterInteract(Entity<PictureTakerComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || args.Target is not { } target || target == args.User)
            return;

        // Don't gate on args.CanReach - that's melee-adjacent range. A camera should be able to
        // snap a picture of someone standing across the room, same as its Flash's own AOE range.
        TryComp<FlashComponent>(ent.Owner, out var flashComp);
        var range = flashComp?.Range ?? 7f;
        if (!_examine.InRangeUnOccluded(args.User, target, range))
            return;

        if (!TryComp<LimitedChargesComponent>(ent.Owner, out var charges) || _charges.IsEmpty((ent.Owner, charges)))
        {
            _popup.PopupClient(Loc.GetString("camera-component-no-film"), args.User);
            return;
        }

        args.Handled = true;
        _charges.TryUseCharge((ent.Owner, charges));

        var flashDuration = TimeSpan.FromSeconds(2);
        var slowTo = 0.8f;
        if (flashComp != null)
        {
            flashDuration = flashComp.AoeFlashDuration;
            slowTo = flashComp.SlowTo;
            _audio.PlayPredicted(flashComp.Sound, ent.Owner, args.User);
        }

        _flash.Flash(target, args.User, ent.Owner, flashDuration, slowTo, displayPopup: true, melee: false);
    }

    // The flash system already handled charges/stun/status-effects for us, we just print the picture.
    private void OnFlashed(Entity<PictureTakerComponent> ent, ref AfterFlashedEvent args)
    {
        TakePicture(ent, args.Target, args.User);
    }

    /// <summary>
    /// Processes entity aimed at with a camera and prints a picture of it.
    /// </summary>
    public void TakePicture(Entity<PictureTakerComponent> camera, EntityUid? target, EntityUid? user)
    {
        if (_net.IsClient)
            return; // Can't interact with predictively spawned entities yet.

        var tableResult = _tables.GetSpawns(camera.Comp.Photographs);
        var coords = Transform(camera).Coordinates;

        FormattedMessage? description = null;
        string? nameText = null;
        if (target != null)
        {
            description = _examine.GetExamineText(target.Value, user, out _);
            // Get the full string now instead of indexing it later because we need the entity to know if it uses a proper noun or not.
            nameText = Loc.GetString("photograph-name-text", ("entity", Identity.Entity(target.Value, EntityManager)));
            // We don't want photographs to contain the descriptions of other photographs, because that makes entities with, in theory, infinite descriptions.
            if (HasComp<PhotographComponent>(target.Value))
            {
                description = null;
                nameText = Loc.GetString("photograph-name-text-photograph");
            }
        }

        foreach (var prototype in tableResult)
        {
            // we generate an individual photograph (there should be only one though)
            var spawned = Spawn(prototype, coords);
            var photoComp = EnsureComp<PhotographComponent>(spawned);
            photoComp.NameText = nameText;
            photoComp.Description = description;
            Dirty(spawned, photoComp);

            _hands.PickupOrDrop(user, spawned, dropNear: true);
        }
    }
}
