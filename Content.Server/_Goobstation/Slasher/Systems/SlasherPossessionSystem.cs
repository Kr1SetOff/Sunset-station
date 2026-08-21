using Content.Server._Goobstation.Slasher.Components;
using Content.Server.Actions;
using Content.Shared._Goobstation.Slasher;
using Content.Shared._Goobstation.Slasher.Components;
using Content.Shared.Coordinates;
using Content.Shared.Examine;
using Content.Shared.Ghost;
using Content.Shared.Mind;
using Content.Shared.Mindshield.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared.Stunnable;
using Robust.Server.Containers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._Goobstation.Slasher.Systems;

/// <summary>
/// Slasher possession. Goob-Station routes this through its shared Devil PossessionSystem, which
/// depends on Devil/Heretic/Shadowling content this fork does not have, so the mind-swap is
/// reimplemented here in a slasher-only form with the same observable behaviour.
/// </summary>
public sealed class SlasherPossessionSystem : EntitySystem
{
    [Dependency] private readonly ActionsSystem _actions = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ContainerSystem _container = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlasherPossessionComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<SlasherPossessionComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<SlasherPossessionComponent, SlasherPossessionEvent>(OnPossess);

        SubscribeLocalEvent<SlasherPossessedComponent, ComponentStartup>(OnPossessedStartup);
        SubscribeLocalEvent<SlasherPossessedComponent, ComponentRemove>(OnPossessedRemoved);
        SubscribeLocalEvent<SlasherPossessedComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<SlasherPossessedComponent, SlasherEndPossessionEvent>(OnEndEarly);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<SlasherPossessedComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_timing.CurTime >= comp.PossessionEndTime)
            {
                RemComp<SlasherPossessedComponent>(uid);
                continue;
            }

            comp.PossessionTimeRemaining = comp.PossessionEndTime - _timing.CurTime;
        }
    }

    private void OnMapInit(Entity<SlasherPossessionComponent> ent, ref MapInitEvent args)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEnt, ent.Comp.ActionId);
    }

    private void OnShutdown(Entity<SlasherPossessionComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEnt);
    }

    /// <summary>
    /// Slasher - Handles the possession of a target.
    /// </summary>
    private void OnPossess(Entity<SlasherPossessionComponent> ent, ref SlasherPossessionEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<MobStateComponent>(args.Target))
            return;

        // Check if the target has a mindshield and return early
        if (ent.Comp.DoesMindshieldBlock && HasComp<MindShieldComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("possession-fail-target-shielded"), ent.Owner, ent.Owner);
            return;
        }

        if (_mobState.IsIncapacitated(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("possession-fail-target-dead"), ent.Owner, ent.Owner);
            return;
        }

        if (HasComp<GhostComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("possession-fail-ghost"), ent.Owner, ent.Owner);
            return;
        }

        if (HasComp<SlasherPossessedComponent>(args.Target))
        {
            _popup.PopupEntity(Loc.GetString("possession-fail-target-already-possessed"), ent.Owner, ent.Owner);
            return;
        }

        if (!_mind.TryGetMind(ent.Owner, out var possessorMind, out _))
            return;

        var possessed = EnsureComp<SlasherPossessedComponent>(args.Target);
        possessed.PossessionEndTime = _timing.CurTime + ent.Comp.PossessionDuration;
        possessed.PossessorOriginalEntity = ent.Owner;
        possessed.PossessorMindId = possessorMind;

        if (_mind.TryGetMind(args.Target, out var targetMind, out _) && possessed.PossessedContainer != null)
        {
            possessed.OriginalMindId = targetMind;

            // Park the victim's mind in a throwaway body so they don't get booted to the lobby.
            var dummy = Spawn("FoodSnackLollypop", MapCoordinates.Nullspace);
            _container.Insert(dummy, possessed.PossessedContainer);
            possessed.DummyEntity = dummy;

            _mind.TransferTo(targetMind, dummy);
        }

        _mind.TransferTo(possessorMind, args.Target);

        _popup.PopupEntity(Loc.GetString("possession-popup-others", ("target", args.Target)), args.Target, PopupType.MediumCaution);
        _audio.PlayPvs(possessed.PossessionSound, args.Target);

        args.Handled = true;
    }

    private void OnPossessedStartup(Entity<SlasherPossessedComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.PossessedContainer = _container.EnsureContainer<Container>(ent, "SlasherPossessedContainer");
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.EndPossessionAction);
    }

    private void OnEndEarly(Entity<SlasherPossessedComponent> ent, ref SlasherEndPossessionEvent args)
    {
        if (args.Handled)
            return;

        RemCompDeferred<SlasherPossessedComponent>(ent);
        args.Handled = true;
    }

    private void OnPossessedRemoved(Entity<SlasherPossessedComponent> ent, ref ComponentRemove args)
    {
        MapCoordinates? coordinates = null;

        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);

        // Return the possessor's mind to their body, and the target to theirs.
        if (!TerminatingOrDeleted(ent.Comp.PossessorMindId) && !TerminatingOrDeleted(ent.Comp.PossessorOriginalEntity))
            _mind.TransferTo(ent.Comp.PossessorMindId, ent.Comp.PossessorOriginalEntity);

        if (!TerminatingOrDeleted(ent.Comp.OriginalMindId))
            _mind.TransferTo(ent.Comp.OriginalMindId, ent.Owner);

        if (!TerminatingOrDeleted(ent.Owner))
            coordinates = _transform.ToMapCoordinates(ent.Owner.ToCoordinates());

        // Paralyze, so you can't just magdump them.
        _stun.TryUpdateParalyzeDuration(ent.Owner, TimeSpan.FromSeconds(10));
        _popup.PopupEntity(Loc.GetString("possession-end-popup", ("target", ent.Owner)), ent.Owner, PopupType.LargeCaution);

        // Teleport to the entity, kinda like you're popping out of their head!
        if (!TerminatingOrDeleted(ent.Comp.PossessorOriginalEntity) && coordinates is not null)
            _transform.SetMapCoordinates(ent.Comp.PossessorOriginalEntity, coordinates.Value);

        if (ent.Comp.DummyEntity is { } dummy && !TerminatingOrDeleted(dummy))
            QueueDel(dummy);
    }

    private void OnExamined(Entity<SlasherPossessedComponent> ent, ref ExaminedEvent args)
    {
        if (!args.IsInDetailsRange
            || args.Examined != args.Examiner)
            return;

        var timeRemaining = Math.Floor(ent.Comp.PossessionTimeRemaining.TotalSeconds);
        args.PushMarkup(Loc.GetString("possessed-component-examined", ("timeremaining", timeRemaining)));
    }
}
