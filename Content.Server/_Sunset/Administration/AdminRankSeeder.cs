// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared.Administration;
using DbAdminRank = Content.Server.Database.AdminRank;

namespace Content.Server._Sunset.Administration;

/// <summary>
///     Создаёт набор стандартных администраторских рангов при старте сервера,
///     если их ещё нет в базе данных. Ранги в этом движке хранятся в БД
///     (таблицы AdminRank/AdminRankFlag) и обычно создаются вручную через
///     внутриигровую панель Permissions — этот сидер делает их доступными
///     "из коробки". Операция идемпотентна: ранг добавляется только если
///     ранга с таким именем ещё нет, поэтому ручные правки флагов не затираются.
/// </summary>
public sealed class AdminRankSeeder
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;

    /// <summary>
    ///     Стандартные ранги и их флаги. Маппинг возможностей на флаги:
    ///     ахелпы — ADMINHELP; aghost / "Обзор" / трюки / открытие админ-меню — ADMIN;
    ///     цвет OOC — NAMECOLOR; баны — BAN (+ заметки для контекста бана);
    ///     дебаг — DEBUG; покарать и выдача антагонистов — FUN;
    ///     спавн существ/предметов — SPAWN; спавн декалей/тайлов — MAPPING;
    ///     перезапуск раунда — ROUND; управление бункером — SERVER;
    ///     модераторские действия (kick/respawn) — MODERATOR.
    /// </summary>
    private static readonly (string Name, AdminFlags Flags)[] DefaultRanks =
    {
        // Ментор: ахелпы, aghost, цвет OOC, баны, бункер, дебаг, трюки.
        ("Ментор",
            AdminFlags.Adminhelp | AdminFlags.Admin | AdminFlags.NameColor | AdminFlags.Ban |
            AdminFlags.Server | AdminFlags.Debug | AdminFlags.Adminchat |
            AdminFlags.ViewNotes | AdminFlags.EditNotes),

        // Младший ивентолог: спавн существ, декали, тайлы, бункер, дебаг, трюки.
        ("Младший ивентолог",
            AdminFlags.Spawn | AdminFlags.Mapping | AdminFlags.Server |
            AdminFlags.Debug | AdminFlags.Admin | AdminFlags.Adminchat),

        // Модерация: декали, тайлы, существа, "Обзор"/aghost/трюки, покарать, дебаг, модерация.
        ("Модерация",
            AdminFlags.Mapping | AdminFlags.Spawn | AdminFlags.Admin | AdminFlags.Fun |
            AdminFlags.Debug | AdminFlags.Moderator | AdminFlags.Adminchat),

        // Ивентолог: полный спавн, "Обзор"/трюки, рестарт раунда, антагонисты, ахелп, дебаг.
        ("Ивентолог",
            AdminFlags.Spawn | AdminFlags.Mapping | AdminFlags.Admin | AdminFlags.Round |
            AdminFlags.Fun | AdminFlags.Adminhelp | AdminFlags.Debug | AdminFlags.Adminchat),

        // Куратор: почти всё, кроме host-only возможностей (в т.ч. выключения сервера — см. engineCommandPerms.yml).
        ("Куратор", AdminFlagsHelper.Everything & ~AdminFlags.Host),
    };

    public async void Initialize()
    {
        _sawmill = _logManager.GetSawmill("admin.rankseeder");

        try
        {
            await SeedRanks();
        }
        catch (Exception e)
        {
            _sawmill.Error($"Не удалось создать стандартные админ-ранги: {e}");
        }
    }

    private async Task SeedRanks()
    {
        var (_, ranks) = await _db.GetAllAdminAndRanksAsync();
        var existing = ranks.Select(r => r.Name).ToHashSet();

        foreach (var (name, flags) in DefaultRanks)
        {
            if (existing.Contains(name))
                continue;

            var rank = new DbAdminRank
            {
                Name = name,
                Flags = AdminFlagsHelper.FlagsToNames(flags)
                    .Select(f => new AdminRankFlag { Flag = f })
                    .ToList(),
            };

            await _db.AddAdminRankAsync(rank);
            _sawmill.Info($"Создан стандартный админ-ранг '{name}'.");
        }
    }
}
