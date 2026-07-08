// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Sunset.NewPlayer;
using Content.Shared.StatusIcon.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Sunset.NewPlayer;

/// <summary>
/// НОВОЕ. Рисует иконку новичка над головой игрока. Иконка видна всем (включая самого игрока),
/// пока сервер держит флаг <see cref="NewPlayerComponent.IsNewbie"/>.
/// </summary>
public sealed class NewPlayerSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NewPlayerComponent, GetStatusIconsEvent>(OnGetStatusIcons);
    }

    private void OnGetStatusIcons(Entity<NewPlayerComponent> ent, ref GetStatusIconsEvent args)
    {
        if (!ent.Comp.IsNewbie)
            return;

        if (_prototype.TryIndex(ent.Comp.Icon, out var icon))
            args.StatusIcons.Add(icon);
    }
}
