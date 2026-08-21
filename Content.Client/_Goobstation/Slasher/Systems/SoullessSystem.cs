using Content.Shared._Goobstation.Slasher.Components;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Slasher.Systems;

/// <summary>
/// Adds a faction icon for soulless entities.
/// </summary>
public sealed class SoullessSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SoullessComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnGetStatusIcons(Entity<SoullessComponent> ent, ref GetStatusIconsEvent args)
    {
        if (_prototype.TryIndex(ent.Comp.FactionIcon, out var iconProto))
            args.StatusIcons.Add(iconProto);
    }
}
