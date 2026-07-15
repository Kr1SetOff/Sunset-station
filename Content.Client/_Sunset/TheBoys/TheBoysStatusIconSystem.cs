using Content.Shared._Sunset.TheBoys.Components;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Sunset.TheBoys;

/// <summary>
/// Lets Butcher's team see each other's TheBoysTeamFaction icon - mirrors
/// Content.Client.Revolutionary.RevolutionarySystem exactly, just for both TheBoys team components
/// instead of one antag/one head-antag pair.
/// </summary>
public sealed class TheBoysStatusIconSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TheBoysButcherComponent, GetStatusIconsEvent>(OnGetIcon);
        SubscribeLocalEvent<TheBoysTeammateComponent, GetStatusIconsEvent>(OnGetIcon);
    }

    private void OnGetIcon(Entity<TheBoysButcherComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.Resolve(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }

    private void OnGetIcon(Entity<TheBoysTeammateComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.Resolve(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
