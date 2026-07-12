// SPDX-FileCopyrightText: 2026 sunset-station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.Genetics.Components;
using Content.Shared.Humanoid;
using Content.Shared.Humanoid.Markings;

namespace Content.Server._Sunset.Genetics;

/// <summary>
///     Enforces <see cref="GeneBaldComponent"/>: strips the carrier's hair and facial hair markings when the
///     gene activates, and restores them again once it deactivates.
/// </summary>
public sealed class GeneBaldSystem : EntitySystem
{
    private static readonly MarkingCategories[] Categories = { MarkingCategories.Hair, MarkingCategories.FacialHair };

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GeneBaldComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<GeneBaldComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<GeneBaldComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
            return;

        foreach (var category in Categories)
        {
            if (!humanoid.MarkingSet.TryGetCategory(category, out var markings) || markings.Count == 0)
                continue;

            ent.Comp.RemovedMarkings[category] = new List<Marking>(markings);
            humanoid.MarkingSet.RemoveCategory(category);
        }

        Dirty(ent.Owner, humanoid);
        var update = new MarkingsUpdateEvent();
        RaiseLocalEvent(ent.Owner, ref update);
    }

    private void OnShutdown(Entity<GeneBaldComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent) || !TryComp<HumanoidAppearanceComponent>(ent, out var humanoid))
            return;

        foreach (var (category, markings) in ent.Comp.RemovedMarkings)
        {
            foreach (var marking in markings)
                humanoid.MarkingSet.AddBack(category, marking);
        }

        Dirty(ent.Owner, humanoid);
        var update = new MarkingsUpdateEvent();
        RaiseLocalEvent(ent.Owner, ref update);
    }
}
