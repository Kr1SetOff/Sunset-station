using Content.Shared._Goobstation.Slasher.Components;
using Content.Shared._Sunset.Grab;
using Content.Shared._Sunset.Grab.Components;
using Content.Shared.Actions;
using Content.Shared.Alert;
using Content.Shared.Movement.Pulling.Systems;
using Content.Shared.Popups;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Timing;

namespace Content.Shared._Goobstation.Slasher.Systems;

/// <summary>
/// Handles the Slasher Relentless Grab action.
/// When activated, the slasher's next melee hit will grab the target.
/// Goob-Station raises its own LightAttackSpecialInteractionEvent for this; Sunset has no such event,
/// so we hook the generic melee hit instead and escalate Sunset's own grab system to Aggressive.
/// </summary>
public sealed class SlasherRelentlessGrabSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly PullingSystem _pulling = default!;
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherRelentlessGrabComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherRelentlessGrabComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherRelentlessGrabComponent, SlasherRelentlessGrabEvent>(OnActivate);
        SubscribeLocalEvent<MeleeWeaponComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMapInit(Entity<SlasherRelentlessGrabComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherRelentlessGrabComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Comp.ActionEnt);
    }

    private void OnActivate(Entity<SlasherRelentlessGrabComponent> ent, ref SlasherRelentlessGrabEvent args)
    {
        if (args.Handled)
            return;

        ent.Comp.Ready = true;
        Dirty(ent);

        _popup.PopupPredicted(Loc.GetString("slasher-relentless-grab-activate"), ent.Owner, ent.Owner);

        args.Handled = true;
    }

    private void OnMeleeHit(Entity<MeleeWeaponComponent> weapon, ref MeleeHitEvent args)
    {
        if (!TryComp<SlasherRelentlessGrabComponent>(args.User, out var grab) || !grab.Ready)
            return;

        EntityUid? target = null;
        foreach (var hit in args.HitEntities)
        {
            target = hit;
            break;
        }

        if (target == null)
            return;

        if (!_pulling.CanPull(args.User, target.Value))
            return;

        if (!_pulling.TryStartPull(args.User, target.Value))
            return;

        EscalateToAggressive(args.User, target.Value);

        grab.Ready = false;
        Dirty(args.User, grab);
    }

    /// <summary>
    /// Mirrors SharedGrabSystem's escalation to the Aggressive stage (Goob's "hard" grab).
    /// </summary>
    private void EscalateToAggressive(EntityUid puller, EntityUid target)
    {
        if (!TryComp<GrabberComponent>(puller, out var grabber)
            || !TryComp<GrabbableComponent>(target, out var grabbable))
            return;

        if (grabber.Stage != GrabStage.Passive)
            return;

        grabber.Stage = GrabStage.Aggressive;
        grabber.NextEscalation = _timing.CurTime + grabber.EscalationCooldown;
        Dirty(puller, grabber);

        grabbable.Stage = GrabStage.Aggressive;
        grabbable.NextChokeTick = _timing.CurTime + grabbable.ChokeTickInterval;
        Dirty(target, grabbable);

        _alerts.ShowAlert(target, grabbable.AggressiveAlert);
    }
}
