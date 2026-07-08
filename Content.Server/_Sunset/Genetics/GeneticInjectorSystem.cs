// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Components;
using Content.Shared.DoAfter;
using Content.Shared._Sunset.Genetics;
using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Robust.Shared.Prototypes;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     Applies a stored genome subset (or a clean-SE wipe) to any body-having creature (humanoid or
///     animal test subject, e.g. monkeys/kobolds) when the injector is used on them.
///     Injection takes a do-after so it can't be applied to everyone instantly.
/// </summary>
public sealed class GeneticInjectorSystem : EntitySystem
{
    [Dependency] private readonly GeneticsSystem _genetics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneticInjectorComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<GeneticInjectorComponent, GeneticInjectorDoAfterEvent>(OnInjected);
    }

    private void OnAfterInteract(Entity<GeneticInjectorComponent> ent, ref AfterInteractEvent args)
    {
        if (args.Handled || !args.CanReach || args.Target is not { } target)
            return;

        if (!HasComp<BodyComponent>(target))
            return;

        args.Handled = true;

        // Injecting yourself is quick; injecting someone else takes the full delay.
        var delay = target == args.User ? ent.Comp.InjectDelay / 2f : ent.Comp.InjectDelay;

        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, args.User, delay,
            new GeneticInjectorDoAfterEvent(), ent, target: target, used: ent)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            NeedHand = true,
            MovementThreshold = 0.5f,
        });
    }

    private void OnInjected(Entity<GeneticInjectorComponent> ent, ref GeneticInjectorDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Args.Target is not { } target)
            return;

        if (!HasComp<BodyComponent>(target))
            return;

        var genome = _genetics.EnsureGenome(target);
        ApplyTo(ent.Comp, genome);
        _genetics.ApplyGenome(target, genome);

        args.Handled = true;
        _popup.PopupEntity(Loc.GetString("dna-injector-used"), target, args.Args.User);

        ent.Comp.Uses--;
        if (ent.Comp.Uses <= 0)
            QueueDel(ent);
        else
            Dirty(ent);
    }

    private void ApplyTo(GeneticInjectorComponent injector, GenomeComponent genome)
    {
        if (injector.CleanSe)
        {
            // Wipe every mutation block but keep the species marker intact.
            for (var i = 1; i < genome.Se.Count; i++)
            {
                if (i == GenomeComponent.BlockSpecies)
                    continue;
                genome.Se[i] = 0;
            }
            return;
        }

        // Gene activator: raise just this mutation's block (wherever this round's shuffled gene
        // layout put it) to its activation threshold. ApplyGenome (called by the caller) then
        // expresses it through the normal mutation path.
        if (injector.ActivateMutation is { } mutationId &&
            _proto.TryIndex<MutationPrototype>(mutationId, out var proto))
        {
            var block = _genetics.GetGeneBlock(proto);
            if (block > 0 && block < genome.Se.Count)
                genome.Se[block] = proto.Disease ? (int) MutationTier.Minor : (int) proto.Tier;
        }

        if (injector.ApplyUi)
            CopyInto(genome.Ui, injector.Ui);
        if (injector.ApplyUe)
            CopyInto(genome.Ue, injector.Ue);
        if (injector.ApplySe)
            CopyInto(genome.Se, injector.Se);
    }

    private static void CopyInto(List<int> destination, List<int> source)
    {
        var count = Math.Min(destination.Count, source.Count);
        for (var i = 0; i < count; i++)
            destination[i] = source[i];
    }
}
