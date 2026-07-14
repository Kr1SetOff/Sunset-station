using Content.Server._Sunset.MartialArts.Components;
using Content.Shared._Sunset.MartialArts;
using Content.Shared._Sunset.MartialArts.Components;
using Content.Shared.NPC.Systems;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.MartialArts.Systems;

/// <summary>
/// Makes Shiva (or anyone else with HatesSleepingCarpComponent) permanently aggro onto any Sleeping
/// Carp practitioner who wanders into range, regardless of faction. Uses NpcFactionSystem's
/// per-entity aggro exception (the same mechanism NPCRetaliationSystem uses), so once someone's
/// been spotted training the style, Shiva won't forget it even if they leave and come back later.
/// </summary>
public sealed partial class HatesSleepingCarpSystem : EntitySystem
{
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var now = _timing.CurTime;
        var query = EntityQueryEnumerator<HatesSleepingCarpComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (now < comp.NextScan)
                continue;

            comp.NextScan = now + comp.ScanInterval;

            var coordinates = Transform(uid).Coordinates;
            var practitioners = new HashSet<Entity<MartialArtsKnowledgeComponent>>();
            _lookup.GetEntitiesInRange(coordinates, comp.ScanRange, practitioners);

            foreach (var practitioner in practitioners)
            {
                if (practitioner.Comp.Style != MartialArtStyle.SleepingCarp)
                    continue;

                _npcFaction.AggroEntity(uid, practitioner.Owner);
            }
        }
    }
}
