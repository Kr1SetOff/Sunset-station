// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Examine;

namespace Content.Shared._Sunset.NewPlayer;

/// <summary>
/// НОВОЕ. Пока игрок считается новичком (<see cref="NewPlayerComponent.IsNewbie"/>), при осмотре
/// показывает, сколько часов он наиграл на сервере. Как только налёт достигает порога и сервер
/// снимает флаг новичка — строка осмотра пропадает автоматически.
/// </summary>
public sealed class SharedNewPlayerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<NewPlayerComponent, ExaminedEvent>(OnExamine);
    }

    private void OnExamine(Entity<NewPlayerComponent> ent, ref ExaminedEvent args)
    {
        if (!ent.Comp.IsNewbie)
            return;

        var hours = ent.Comp.Playtime.TotalHours;
        args.PushMarkup(Loc.GetString("new-player-examine", ("hours", Math.Round(hours, 1))));
    }
}
