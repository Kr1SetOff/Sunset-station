// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Server.Humanoid;
using Content.Shared.Actions;
using Content.Shared._Sunset.Genetics;
using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Humanoid;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     Core of the genetics system. Generates a genome for humanoids, applies the UI blocks to their
///     appearance, the UE blocks to their identity, and expresses/represses SE-block mutations.
///     Instability effects are handled in the partial <c>GeneticsSystem.Instability.cs</c>.
/// </summary>
public sealed partial class GeneticsSystem : SharedGeneticsSystem
{
    [Dependency] private readonly HumanoidAppearanceSystem _humanoid = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    // Deterministic name pools so that the same UE always yields the same identity.
    private static readonly string[] FirstNames =
    {
        "Alex", "Morgan", "Casey", "Jordan", "Riley", "Quinn", "Avery", "Skyler",
        "Drew", "Rowan", "Emerson", "Sage", "Reese", "Hayden", "Phoenix", "Lane",
    };

    private static readonly string[] LastNames =
    {
        "Vance", "Holloway", "Crane", "Mercer", "Ashford", "Doyle", "Sloan", "Verne",
        "Hart", "Kessler", "Mires", "Voss", "Quill", "Renner", "Stark", "Wren",
    };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidAppearanceComponent, MapInitEvent>(OnHumanoidMapInit);
        InitializeInstability();
    }

    private void OnHumanoidMapInit(Entity<HumanoidAppearanceComponent> ent, ref MapInitEvent args)
    {
        var genome = EnsureComp<GenomeComponent>(ent);
        if (!genome.Generated)
            GenerateGenome(ent, genome, ent.Comp);
    }

    /// <summary>Creates a fresh, randomized genome that encodes the entity's current appearance.</summary>
    public void GenerateGenome(EntityUid uid, GenomeComponent genome, HumanoidAppearanceComponent humanoid)
    {
        genome.Ui = NewBlocks(GenomeComponent.UiBlockCount);
        genome.Ue = NewBlocks(GenomeComponent.UeBlockCount);
        genome.Se = NewBlocks(GenomeComponent.SeBlockCount);

        // Encode current appearance into the UI blocks so editing them has a visible effect.
        EncodeColor(genome.Ui, GenomeComponent.BlockSkinR, humanoid.SkinColor);
        EncodeColor(genome.Ui, GenomeComponent.BlockEyeR, humanoid.EyeColor);
        genome.Ui[GenomeComponent.BlockSex] = humanoid.Sex == Sex.Female ? 0x100 : 0xC00;

        // Randomize the identity enzymes; remember the hash so we don't rename at spawn.
        for (var i = 0; i < genome.Ue.Count; i++)
            genome.Ue[i] = _random.Next(0, GenomeComponent.MaxBlock + 1);
        genome.AppliedUeHash = HashBlocks(genome.Ue);

        // Randomize structural enzymes below the activation thresholds, so nobody spawns with random
        // powers; the blocks still vary so pulse radiation can discover them.
        for (var i = 1; i < genome.Se.Count; i++)
            genome.Se[i] = _random.Next(0, (int) MutationTier.Minor);

        genome.Se[GenomeComponent.BlockSpecies] = 0x320; // humanoid marker (< 0x320 boundary in SS13)

        genome.Generated = true;
        Dirty(uid, genome);
    }

    /// <summary>
    ///     Applies the genome to the carrier: UI to appearance, UE to name, SE to mutations, then
    ///     recomputes instability. Call this after any genome change.
    /// </summary>
    public void ApplyGenome(EntityUid uid, GenomeComponent? genome = null)
    {
        if (!Resolve(uid, ref genome))
            return;

        if (TryComp<HumanoidAppearanceComponent>(uid, out var humanoid))
            ApplyAppearance(uid, genome, humanoid);

        ApplyMutations(uid, genome);
        RecomputeInstability(genome);
        Dirty(uid, genome);
    }

    private void ApplyAppearance(EntityUid uid, GenomeComponent genome, HumanoidAppearanceComponent humanoid)
    {
        var skin = BlocksToColor(
            Get(genome.Ui, GenomeComponent.BlockSkinR),
            Get(genome.Ui, GenomeComponent.BlockSkinG),
            Get(genome.Ui, GenomeComponent.BlockSkinB));
        _humanoid.SetSkinColor(uid, skin, humanoid: humanoid);

        var eyes = BlocksToColor(
            Get(genome.Ui, GenomeComponent.BlockEyeR),
            Get(genome.Ui, GenomeComponent.BlockEyeG),
            Get(genome.Ui, GenomeComponent.BlockEyeB));
        _humanoid.SetBaseLayerColor(uid, HumanoidVisualLayers.Eyes, eyes, humanoid: humanoid);

        var sex = Get(genome.Ui, GenomeComponent.BlockSex) < GenomeComponent.SexThreshold ? Sex.Female : Sex.Male;
        _humanoid.SetSex(uid, sex, humanoid: humanoid);

        // Identity: regenerate the name only when the UE blocks have actually changed.
        var ueHash = HashBlocks(genome.Ue);
        if (ueHash != genome.AppliedUeHash)
        {
            _metaData.SetEntityName(uid, GenerateName(genome.Ue));
            genome.AppliedUeHash = ueHash;
        }
    }

    private void ApplyMutations(EntityUid uid, GenomeComponent genome)
    {
        foreach (var proto in _proto.EnumeratePrototypes<MutationPrototype>())
        {
            if (proto.Block <= 0 || proto.Block >= genome.Se.Count)
                continue;

            var shouldBeActive = IsMutationActive(genome.Se[proto.Block], proto.Tier, proto.Disease);
            var isActive = genome.ActiveMutations.Contains(proto.ID);

            if (shouldBeActive && !isActive)
                ActivateMutation(uid, genome, proto);
            else if (!shouldBeActive && isActive)
                DeactivateMutation(uid, genome, proto);
        }
    }

    public void ActivateMutation(EntityUid uid, GenomeComponent genome, MutationPrototype proto)
    {
        if (genome.ActiveMutations.Contains(proto.ID))
            return;

        if (proto.Components.Count > 0)
            EntityManager.AddComponents(uid, proto.Components);

        var granted = new List<EntityUid>();
        foreach (var actionId in proto.Actions)
        {
            if (_actions.AddAction(uid, actionId) is { } action)
                granted.Add(action);
        }

        genome.GrantedActions[proto.ID] = granted;
        genome.ActiveMutations.Add(proto.ID);
    }

    public void DeactivateMutation(EntityUid uid, GenomeComponent genome, MutationPrototype proto)
    {
        if (!genome.ActiveMutations.Remove(proto.ID))
            return;

        if (proto.Components.Count > 0)
            EntityManager.RemoveComponents(uid, proto.Components);

        if (genome.GrantedActions.Remove(proto.ID, out var actions))
        {
            foreach (var action in actions)
                _actions.RemoveAction(action);
        }
    }

    private void RecomputeInstability(GenomeComponent genome)
    {
        var instability = 0f;
        foreach (var id in genome.ActiveMutations)
        {
            if (_proto.TryIndex<MutationPrototype>(id, out var proto))
                instability += proto.Instability;
        }

        genome.Instability = instability;
    }

    private List<int> NewBlocks(int count)
    {
        var list = new List<int>(count);
        for (var i = 0; i < count; i++)
            list.Add(0);
        return list;
    }

    private static int Get(List<int> blocks, int index)
    {
        return index >= 0 && index < blocks.Count ? blocks[index] : 0;
    }

    private static void EncodeColor(List<int> blocks, int firstBlock, Color color)
    {
        if (firstBlock + 2 >= blocks.Count)
            return;

        blocks[firstBlock] = ChannelToBlock(color.RByte);
        blocks[firstBlock + 1] = ChannelToBlock(color.GByte);
        blocks[firstBlock + 2] = ChannelToBlock(color.BByte);
    }

    private static string GenerateName(List<int> ue)
    {
        var first = FirstNames[Math.Abs(Get(ue, 0)) % FirstNames.Length];
        var last = LastNames[Math.Abs(Get(ue, 1)) % LastNames.Length];
        return $"{first} {last}";
    }
}
