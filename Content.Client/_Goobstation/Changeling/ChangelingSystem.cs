// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Goobstation.Changeling.Components;
using Content.Shared._Goobstation.Changeling.Systems;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Goobstation.Changeling;

public sealed class ChangelingSystem : SharedChangelingSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingIdentityComponent, GetStatusIconsEvent>(GetChanglingIcon);
    }

    private void GetChanglingIcon(Entity<ChangelingIdentityComponent> ent, ref GetStatusIconsEvent args)
    {
        if (HasComp<HivemindComponent>(ent) && _prototype.TryIndex(ent.Comp.StatusIcon, out var iconPrototype))
            args.StatusIcons.Add(iconPrototype);
    }
}
