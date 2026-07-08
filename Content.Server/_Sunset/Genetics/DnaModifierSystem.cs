// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.FixedPoint;
using Content.Server.DeviceLinking.Systems;
using Content.Server._Sunset.Genetics.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Body.Components;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Content.Shared.Climbing.Systems;
using Content.Shared.Damage;
using Content.Shared.Damage.Systems;
using Content.Shared.Destructible;
using Content.Shared.DeviceLinking.Events;
using Content.Shared.DragDrop;
using Content.Shared._Sunset.Genetics;
using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Humanoid;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Events;
using Content.Shared.Verbs;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Random;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     The DNA modifier machine: holds a humanoid occupant and lets a console irradiate their genome.
///     Mirrors <see cref="Content.Server.Medical.MedicalScannerSystem"/>.
/// </summary>
public sealed class DnaModifierSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
    [Dependency] private readonly ActionBlockerSystem _blocker = default!;
    [Dependency] private readonly ClimbSystem _climbSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GeneticsSystem _genetics = default!;
    [Dependency] private readonly DnaModifierConsoleSystem _console = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    private const float UpdateRate = 1f;
    private float _updateDif;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DnaModifierComponent, ComponentInit>(OnComponentInit);
        SubscribeLocalEvent<DnaModifierComponent, ContainerRelayMovementEntityEvent>(OnRelayMovement);
        SubscribeLocalEvent<DnaModifierComponent, GetVerbsEvent<InteractionVerb>>(AddInsertOtherVerb);
        SubscribeLocalEvent<DnaModifierComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        SubscribeLocalEvent<DnaModifierComponent, DestructionEventArgs>(OnDestroyed);
        SubscribeLocalEvent<DnaModifierComponent, DragDropTargetEvent>(OnDragDropOn, after: [typeof(ClimbSystem)]);
        SubscribeLocalEvent<DnaModifierComponent, CanDropTargetEvent>(OnCanDragDropOn);
        SubscribeLocalEvent<DnaModifierComponent, PortDisconnectedEvent>(OnPortDisconnected);
        SubscribeLocalEvent<DnaModifierComponent, AnchorStateChangedEvent>(OnAnchorChanged);
        SubscribeLocalEvent<DnaModifierComponent, ClimbedOnEvent>(OnClimbedOn);
    }

    private void OnComponentInit(EntityUid uid, DnaModifierComponent component, ComponentInit args)
    {
        component.BodyContainer = _containerSystem.EnsureContainer<ContainerSlot>(uid, "dnamod-bodyContainer");
        _signalSystem.EnsureSinkPorts(uid, DnaModifierComponent.MachinePort);
    }

    private void OnCanDragDropOn(EntityUid uid, DnaModifierComponent component, ref CanDropTargetEvent args)
    {
        args.Handled = true;
        args.CanDrop = CanInsert(uid, args.Dragged, component);
    }

    public bool CanInsert(EntityUid uid, EntityUid target, DnaModifierComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return false;

        if (IsOccupied(component))
            return false;

        // Allow any entity with a body component (humans, monkeys, kobolds, etc.)
        return HasComp<BodyComponent>(target);
    }

    public static bool IsOccupied(DnaModifierComponent component)
    {
        return component.BodyContainer.ContainedEntity != null;
    }

    private void OnRelayMovement(EntityUid uid, DnaModifierComponent component, ref ContainerRelayMovementEntityEvent args)
    {
        if (!_blocker.CanInteract(args.Entity, uid))
            return;

        EjectBody(uid, component);
    }

    private void AddInsertOtherVerb(EntityUid uid, DnaModifierComponent component, GetVerbsEvent<InteractionVerb> args)
    {
        if (args.Using == null ||
            !args.CanAccess ||
            !args.CanInteract ||
            IsOccupied(component) ||
            !CanInsert(uid, args.Using.Value, component))
            return;

        var name = "Unknown";
        if (TryComp(args.Using.Value, out MetaDataComponent? metadata))
            name = metadata.EntityName;

        var target = args.Target;
        var user = args.Using.Value;
        args.Verbs.Add(new InteractionVerb
        {
            Act = () => InsertBody(uid, user, component),
            Category = VerbCategory.Insert,
            Text = name,
        });
    }

    private void AddAlternativeVerbs(EntityUid uid, DnaModifierComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !args.CanComplexInteract)
            return;

        if (IsOccupied(component))
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Act = () => EjectBody(uid, component),
                Category = VerbCategory.Eject,
                Text = Loc.GetString("dna-modifier-verb-noun-occupant"),
                Priority = 1,
            });
        }

        if (!IsOccupied(component) && CanInsert(uid, args.User, component) && _blocker.CanMove(args.User))
        {
            var user = args.User;
            args.Verbs.Add(new AlternativeVerb
            {
                Act = () => InsertBody(uid, user, component),
                Text = Loc.GetString("dna-modifier-verb-enter"),
            });
        }
    }

    private void OnDestroyed(EntityUid uid, DnaModifierComponent component, DestructionEventArgs args)
    {
        EjectBody(uid, component);
    }

    private void OnDragDropOn(EntityUid uid, DnaModifierComponent component, ref DragDropTargetEvent args)
    {
        if (args.Handled)
            return;

        InsertBody(uid, args.Dragged, component);
        // Mark handled so nothing else reacts to the drag-drop on the now-contained body.
        args.Handled = true;
    }

    private void OnPortDisconnected(EntityUid uid, DnaModifierComponent component, PortDisconnectedEvent args)
    {
        component.ConnectedConsole = null;
    }

    private void OnAnchorChanged(EntityUid uid, DnaModifierComponent component, ref AnchorStateChangedEvent args)
    {
        if (component.ConnectedConsole is not { } console || !TryComp<DnaModifierConsoleComponent>(console, out var consoleComp))
            return;

        if (args.Anchored)
            _console.RecheckConnections(console, uid, consoleComp);
        else
            _console.UpdateUserInterface(console, consoleComp);
    }

    private void OnClimbedOn(EntityUid uid, DnaModifierComponent component, ref ClimbedOnEvent args)
    {
        // Не засовывать обратно тело, которое прямо сейчас извлекается:
        // EjectBody вызывает ForciblySetClimbing, что поднимает ClimbedOnEvent.
        if (component.Ejecting)
            return;

        // Когда кто-то взбирается на капсулу, засовываем его внутрь.
        // Контейнерная вставка сама снимает состояние карабканья - отдельный вызов не нужен.
        InsertBody(uid, args.Climber, component);
    }

    public void InsertBody(EntityUid uid, EntityUid toInsert, DnaModifierComponent? component)
    {
        if (!Resolve(uid, ref component))
            return;

        if (IsOccupied(component) || !CanInsert(uid, toInsert, component))
            return;

        _containerSystem.Insert(toInsert, component.BodyContainer);

        // Humanoids already have a genome from spawn; non-humanoid test subjects (monkeys, kobolds, ...)
        // get theirs generated here, the first time they're actually scanned.
        _genetics.EnsureGenome(toInsert);

        UpdateAppearance(uid, component);

        if (component.ConnectedConsole is { } console && TryComp<DnaModifierConsoleComponent>(console, out var consoleComp))
            _console.UpdateUserInterface(console, consoleComp);
    }

    public void EjectBody(EntityUid uid, DnaModifierComponent? component)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.BodyContainer.ContainedEntity is not { Valid: true } contained)
            return;

        component.Ejecting = true;
        _containerSystem.Remove(contained, component.BodyContainer);
        _climbSystem.ForciblySetClimbing(contained, uid);
        component.Ejecting = false;

        UpdateAppearance(uid, component);

        if (component.ConnectedConsole is { } console && TryComp<DnaModifierConsoleComponent>(console, out var consoleComp))
            _console.UpdateUserInterface(console, consoleComp);
    }

    private DnaModifierStatus GetStatus(DnaModifierComponent component)
    {
        if (component.BodyContainer.ContainedEntity is not { } body)
            return DnaModifierStatus.Idle;

        // A subject that can no longer be alive (dead body) shows the gore state.
        if (TryComp<MobStateComponent>(body, out var state) && _mobState.IsDead(body, state))
            return DnaModifierStatus.Gore;

        return DnaModifierStatus.Occupied;
    }

    private void UpdateAppearance(EntityUid uid, DnaModifierComponent component)
    {
        _appearance.SetData(uid, DnaModifierVisuals.Status, GetStatus(component));
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        _updateDif += frameTime;
        if (_updateDif < UpdateRate)
            return;

        _updateDif -= UpdateRate;

        // Refresh sprites so an occupant dying inside the machine flips it to the gore state.
        var query = EntityQueryEnumerator<DnaModifierComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            UpdateAppearance(uid, component);
        }
    }

    /// <summary>
    ///     Fire a targeted radiation pulse at one subblock of the occupant's genome, Paradise SS13
    ///     style: no precise editing. The beam usually hits the aimed nibble but can drift onto a
    ///     neighbouring subblock or block, and the resulting hex digit is random - the geneticist
    ///     keeps pulsing (paying radiation/toxin damage to the subject each time) until the digit
    ///     lands high enough for the gene tier they're after (>= D for major powers).
    /// </summary>
    public void Radiate(EntityUid uid, GenomeCategory category, int block, int subBlock, DnaModifierComponent? component = null)
    {
        if (!Resolve(uid, ref component) ||
            component.BodyContainer.ContainedEntity is not { } occupant ||
            !TryComp<GenomeComponent>(occupant, out var genome))
            return;

        var blocks = GetBlocks(genome, category);
        if (block < 0 || block >= blocks.Count)
            return;

        subBlock = Math.Clamp(subBlock, 0, 2);

        // Beam inaccuracy: sometimes the pulse lands next to where it was aimed.
        var roll = _random.NextFloat();
        if (roll < component.DriftSubBlockChance)
            subBlock = Math.Clamp(subBlock + (_random.Prob(0.5f) ? 1 : -1), 0, 2);
        else if (roll < component.DriftSubBlockChance + component.DriftBlockChance)
            block = Math.Clamp(block + (_random.Prob(0.5f) ? 1 : -1), 0, blocks.Count - 1);

        blocks[block] = SharedGeneticsSystem.SetSubBlock(blocks[block], subBlock, _random.Next(0x10));

        FlipExtraSubblock(blocks, component);
        Hurt(occupant, component);
        _genetics.ApplyGenome(occupant, genome);
        RefreshConsole(component);
    }

    /// <summary>Randomly mutate one block within a category (SS13 "Pulse Radiation").</summary>
    public void Pulse(EntityUid uid, GenomeCategory category, DnaModifierComponent? component = null)
    {
        if (!Resolve(uid, ref component) ||
            component.BodyContainer.ContainedEntity is not { } occupant ||
            !TryComp<GenomeComponent>(occupant, out var genome))
            return;

        var blocks = GetBlocks(genome, category);
        if (blocks.Count == 0)
            return;

        var block = _random.Next(blocks.Count);
        var sub = _random.Next(3);
        blocks[block] = SharedGeneticsSystem.SetSubBlock(blocks[block], sub, _random.Next(0x10));

        FlipExtraSubblock(blocks, component);
        Hurt(occupant, component);
        _genetics.ApplyGenome(occupant, genome);
        RefreshConsole(component);
    }

    private void FlipExtraSubblock(List<int> blocks, DnaModifierComponent component)
    {
        if (blocks.Count == 0 || !_random.Prob(component.ExtraMutationChance))
            return;

        var block = _random.Next(blocks.Count);
        var sub = _random.Next(3);
        blocks[block] = SharedGeneticsSystem.SetSubBlock(blocks[block], sub, _random.Next(0x10));
    }

    private void Hurt(EntityUid occupant, DnaModifierComponent component)
    {
        var damage = new DamageSpecifier
        {
            DamageDict = new Dictionary<string, FixedPoint2>
            {
                { "Poison", FixedPoint2.New(component.ToxinPerPulse) },
                { "Radiation", FixedPoint2.New(component.RadiationPerPulse) },
            },
        };
        _damageable.TryChangeDamage(occupant, damage);
    }

    private void RefreshConsole(DnaModifierComponent component)
    {
        if (component.ConnectedConsole is { } console && TryComp<DnaModifierConsoleComponent>(console, out var consoleComp))
            _console.UpdateUserInterface(console, consoleComp);
    }

    private static List<int> GetBlocks(GenomeComponent genome, GenomeCategory category)
    {
        return category switch
        {
            GenomeCategory.Ui => genome.Ui,
            GenomeCategory.Ue => genome.Ue,
            GenomeCategory.Se => genome.Se,
            _ => genome.Se,
        };
    }
}
