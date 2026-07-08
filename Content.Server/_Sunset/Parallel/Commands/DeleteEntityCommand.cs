// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;

namespace Content.Server._Sunset.Parallel.Commands;

/// <summary>
/// Удалить сущность. Решение «эта сущность должна исчезнуть» может быть принято
/// в фоне (например, полностью сгоревший предмет), но структурное изменение —
/// только главный поток.
///
/// Используется QueueDel, а не Del: удаление откладывается до безопасной точки
/// тика движком. Прямое Del из произвольного места тоже небезопасно посреди
/// итерации систем.
/// </summary>
public readonly record struct DeleteEntityCommand(EntityUid Uid) : IWorldCommand
{
    public void Apply(in ParallelApplyContext ctx)
    {
        ctx.EntityManager.QueueDeleteEntity(Uid);
    }
}
