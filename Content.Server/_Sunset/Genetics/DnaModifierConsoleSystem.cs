// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server._Sunset.Genetics.Components;
using Content.Server.DeviceLinking.Systems;
using Content.Server.Power.EntitySystems;
using Content.Shared.DeviceLinking;
using Content.Shared.DeviceLinking.Events;
using Content.Shared._Sunset.Genetics;
using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Popups;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     Drives the DNA modifier console UI and links it to its machine. Mirrors the cloning console.
/// </summary>
public sealed class DnaModifierConsoleSystem : EntitySystem
{
    [Dependency] private readonly DeviceLinkSystem _signalSystem = default!;
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly DnaModifierSystem _machine = default!;
    [Dependency] private readonly GeneticsSystem _genetics = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<DnaModifierConsoleComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<DnaModifierConsoleComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<DnaModifierConsoleComponent, AfterActivatableUIOpenEvent>(OnUIOpen);
        SubscribeLocalEvent<DnaModifierConsoleComponent, PowerChangedEvent>(OnPowerChanged);
        SubscribeLocalEvent<DnaModifierConsoleComponent, NewLinkEvent>(OnNewLink);
        SubscribeLocalEvent<DnaModifierConsoleComponent, PortDisconnectedEvent>(OnPortDisconnected);
        SubscribeLocalEvent<DnaModifierConsoleComponent, AnchorStateChangedEvent>(OnAnchorChanged);

        SubscribeLocalEvent<DnaModifierConsoleComponent, DnaModifierRadiateMessage>(OnRadiate);
        SubscribeLocalEvent<DnaModifierConsoleComponent, DnaModifierPulseMessage>(OnPulse);
        SubscribeLocalEvent<DnaModifierConsoleComponent, DnaModifierEjectMessage>(OnEject);
        SubscribeLocalEvent<DnaModifierConsoleComponent, DnaModifierBufferMessage>(OnBuffer);
        SubscribeLocalEvent<DnaModifierConsoleComponent, DnaModifierPrintActivatorMessage>(OnPrintActivator);
    }

    private void OnInit(EntityUid uid, DnaModifierConsoleComponent component, ComponentInit args)
    {
        _signalSystem.EnsureSourcePorts(uid, DnaModifierConsoleComponent.MachinePort);
    }

    private void OnMapInit(EntityUid uid, DnaModifierConsoleComponent component, MapInitEvent args)
    {
        if (!TryComp<DeviceLinkSourceComponent>(uid, out var source))
            return;

        foreach (var port in source.Outputs.Values.SelectMany(ports => ports))
        {
            if (!TryComp<DnaModifierComponent>(port, out var machine))
                continue;

            component.Machine = port;
            machine.ConnectedConsole = uid;
        }
    }

    private void OnNewLink(EntityUid uid, DnaModifierConsoleComponent component, NewLinkEvent args)
    {
        if (args.SourcePort == DnaModifierConsoleComponent.MachinePort && TryComp<DnaModifierComponent>(args.Sink, out var machine))
        {
            component.Machine = args.Sink;
            machine.ConnectedConsole = uid;
        }

        RecheckConnections(uid, component.Machine, component);
    }

    private void OnPortDisconnected(EntityUid uid, DnaModifierConsoleComponent component, PortDisconnectedEvent args)
    {
        if (args.Port == DnaModifierConsoleComponent.MachinePort)
            component.Machine = null;

        UpdateUserInterface(uid, component);
    }

    private void OnUIOpen(EntityUid uid, DnaModifierConsoleComponent component, AfterActivatableUIOpenEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    private void OnPowerChanged(EntityUid uid, DnaModifierConsoleComponent component, ref PowerChangedEvent args)
    {
        UpdateUserInterface(uid, component);
    }

    private void OnAnchorChanged(EntityUid uid, DnaModifierConsoleComponent component, ref AnchorStateChangedEvent args)
    {
        if (args.Anchored)
            RecheckConnections(uid, component.Machine, component);
        else
            UpdateUserInterface(uid, component);
    }

    private void OnRadiate(EntityUid uid, DnaModifierConsoleComponent component, DnaModifierRadiateMessage args)
    {
        if (!_powerReceiver.IsPowered(uid) || !component.MachineInRange || component.Machine is not { } machine)
            return;

        _machine.Radiate(machine, args.Category, args.Block, args.SubBlock);
    }

    private void OnPulse(EntityUid uid, DnaModifierConsoleComponent component, DnaModifierPulseMessage args)
    {
        if (!_powerReceiver.IsPowered(uid) || !component.MachineInRange || component.Machine is not { } machine)
            return;

        _machine.Pulse(machine, args.Category);
    }

    private void OnEject(EntityUid uid, DnaModifierConsoleComponent component, DnaModifierEjectMessage args)
    {
        if (component.Machine is { } machine)
            _machine.EjectBody(machine, null);
    }

    private void OnPrintActivator(EntityUid uid, DnaModifierConsoleComponent component, DnaModifierPrintActivatorMessage args)
    {
        // Only print activators for mutations this console has actually researched.
        if (!_powerReceiver.IsPowered(uid) || !component.DiscoveredMutations.Contains(args.MutationId))
            return;

        // Enforce the print cooldown so activator injectors can't be spammed.
        var now = _timing.CurTime;
        if (now < component.NextPrint)
        {
            var remaining = (int) (component.NextPrint - now).TotalSeconds;
            _popup.PopupCursor(Loc.GetString("dna-modifier-console-print-cooldown", ("seconds", remaining)),
                args.Actor, PopupType.MediumCaution);
            return;
        }

        component.NextPrint = now + component.PrintCooldown;
        SpawnActivatorInjector(uid, component, args.MutationId);
    }

    private void SpawnActivatorInjector(EntityUid console, DnaModifierConsoleComponent component, string mutationId)
    {
        var injector = Spawn(component.ActivatorInjectorPrototype, Transform(console).Coordinates);
        var comp = EnsureComp<GeneticInjectorComponent>(injector);

        comp.ActivateMutation = mutationId;
        comp.Uses = 1;
        Dirty(injector, comp);
    }

    private void OnBuffer(EntityUid uid, DnaModifierConsoleComponent component, DnaModifierBufferMessage args)
    {
        if (!_powerReceiver.IsPowered(uid) || args.Slot < 0 || args.Slot >= component.Buffers.Length)
            return;

        switch (args.Action)
        {
            case DnaBufferAction.Clear:
                component.Buffers[args.Slot] = null;
                break;

            case DnaBufferAction.SaveSe:
            case DnaBufferAction.SaveUi:
            case DnaBufferAction.SaveUiUe:
                if (TryGetOccupantGenome(component, out _, out var saveGenome))
                    component.Buffers[args.Slot] = MakeSnapshot(saveGenome, args.Action);
                break;

            case DnaBufferAction.Apply:
                if (component.Buffers[args.Slot] is { } applyBuffer &&
                    TryGetOccupantGenome(component, out var occupant, out var applyGenome))
                {
                    ApplySnapshot(applyBuffer, applyGenome);
                    _genetics.ApplyGenome(occupant, applyGenome);
                }
                break;

            case DnaBufferAction.Injector:
                if (component.Buffers[args.Slot] is { } injectorBuffer)
                    SpawnInjector(uid, component, injectorBuffer);
                break;
        }

        UpdateUserInterface(uid, component);
    }

    private bool TryGetOccupantGenome(DnaModifierConsoleComponent component, out EntityUid occupant, out GenomeComponent genome)
    {
        occupant = default;
        genome = default!;

        if (component.Machine is not { } machine || !TryComp<DnaModifierComponent>(machine, out var modifier))
            return false;

        if (modifier.BodyContainer.ContainedEntity is not { } body || !TryComp(body, out GenomeComponent? found))
            return false;

        occupant = body;
        genome = found;
        return true;
    }

    private static GenomeSnapshot MakeSnapshot(GenomeComponent genome, DnaBufferAction action)
    {
        var snapshot = new GenomeSnapshot();

        if (action is DnaBufferAction.SaveUi or DnaBufferAction.SaveUiUe)
        {
            snapshot.Ui = new List<int>(genome.Ui);
            snapshot.HasUi = true;
        }

        if (action is DnaBufferAction.SaveUiUe)
        {
            snapshot.Ue = new List<int>(genome.Ue);
            snapshot.HasUe = true;
        }

        if (action is DnaBufferAction.SaveSe)
        {
            snapshot.Se = new List<int>(genome.Se);
            snapshot.HasSe = true;
        }

        return snapshot;
    }

    private static void ApplySnapshot(GenomeSnapshot snapshot, GenomeComponent genome)
    {
        if (snapshot.HasUi)
            CopyInto(genome.Ui, snapshot.Ui);
        if (snapshot.HasUe)
            CopyInto(genome.Ue, snapshot.Ue);
        if (snapshot.HasSe)
            CopyInto(genome.Se, snapshot.Se);
    }

    private static void CopyInto(List<int> destination, List<int> source)
    {
        var count = Math.Min(destination.Count, source.Count);
        for (var i = 0; i < count; i++)
            destination[i] = source[i];
    }

    private void SpawnInjector(EntityUid console, DnaModifierConsoleComponent component, GenomeSnapshot snapshot)
    {
        var injector = Spawn(component.InjectorPrototype, Transform(console).Coordinates);
        var comp = EnsureComp<GeneticInjectorComponent>(injector);

        comp.ApplyUi = snapshot.HasUi;
        comp.ApplyUe = snapshot.HasUe;
        comp.ApplySe = snapshot.HasSe;
        comp.Ui = new List<int>(snapshot.Ui);
        comp.Ue = new List<int>(snapshot.Ue);
        comp.Se = new List<int>(snapshot.Se);
        Dirty(injector, comp);
    }

    public void RecheckConnections(EntityUid console, EntityUid? machine, DnaModifierConsoleComponent? component = null)
    {
        if (!Resolve(console, ref component))
            return;

        if (machine != null)
        {
            Transform(machine.Value).Coordinates.TryDistance(EntityManager, Transform(console).Coordinates, out var distance);
            component.MachineInRange = distance <= component.MaxDistance;
        }

        UpdateUserInterface(console, component);
    }

    public void UpdateUserInterface(EntityUid uid, DnaModifierConsoleComponent component)
    {
        if (!_uiSystem.HasUi(uid, DnaModifierConsoleUiKey.Key))
            return;

        if (!_powerReceiver.IsPowered(uid))
        {
            _uiSystem.CloseUis(uid);
            return;
        }

        RecordDiscoveries(component);
        _uiSystem.SetUiState(uid, DnaModifierConsoleUiKey.Key, BuildState(component));
    }

    /// <summary>
    ///     Refreshes the console's mutation list to exactly what the current occupant expresses. Nothing is
    ///     persisted: a mutation that leaves the subject (or an empty machine) clears from the console too.
    /// </summary>
    private void RecordDiscoveries(DnaModifierConsoleComponent component)
    {
        component.DiscoveredMutations.Clear();
        if (TryGetOccupantGenome(component, out _, out var genome))
            component.DiscoveredMutations.UnionWith(genome.ActiveMutations);
    }

    private DnaModifierConsoleBoundUserInterfaceState BuildState(DnaModifierConsoleComponent component)
    {
        var connected = false;
        var hasOccupant = false;
        var occupantName = Loc.GetString("generic-unknown");
        var ui = new List<int>();
        var ue = new List<int>();
        var se = new List<int>();
        var instability = 0f;
        var mutations = new List<string>();
        var activeMutationIds = new HashSet<string>();

        if (component.Machine is { } machine && TryComp<DnaModifierComponent>(machine, out var modifier))
        {
            connected = true;

            if (modifier.BodyContainer.ContainedEntity is { } occupant && TryComp<GenomeComponent>(occupant, out var genome))
            {
                hasOccupant = true;
                occupantName = MetaData(occupant).EntityName;
                ui = genome.Ui;
                ue = genome.Ue;
                se = genome.Se;
                instability = genome.Instability;

                foreach (var id in genome.ActiveMutations)
                {
                    activeMutationIds.Add(id);
                    mutations.Add(_proto.TryIndex<MutationPrototype>(id, out var proto)
                        ? Loc.GetString(proto.Name)
                        : id);
                }
            }
        }

        var bufferFilled = new List<bool>(component.Buffers.Length);
        foreach (var buffer in component.Buffers)
            bufferFilled.Add(buffer != null);

        var discovered = new List<DiscoveredMutationInfo>();
        foreach (var id in component.DiscoveredMutations)
        {
            if (!_proto.TryIndex<MutationPrototype>(id, out var proto))
                continue;

            discovered.Add(new DiscoveredMutationInfo(
                id,
                Loc.GetString(proto.Name),
                Loc.GetString(proto.Description),
                activeMutationIds.Contains(id)));
        }

        return new DnaModifierConsoleBoundUserInterfaceState(
            connected,
            component.MachineInRange,
            hasOccupant,
            occupantName,
            ui,
            ue,
            se,
            instability,
            mutations,
            bufferFilled,
            discovered);
    }
}
