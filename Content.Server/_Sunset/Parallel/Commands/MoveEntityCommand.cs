// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.Server._Sunset.Parallel.Commands;

/// <summary>
/// Переместить сущность в заданные координаты. Новую позицию фоновый поток
/// считает сам (например, вектор стиринга NPC или сдвиг по давлению), но
/// применить может только главный поток.
///
/// Почему ТОЛЬКО главный поток: SetCoordinates меняет дерево трансформов и
/// поднимает MoveEvent — параллельная запись в дерево приведёт к порче данных.
/// </summary>
public readonly record struct MoveEntityCommand(
    EntityUid Uid,
    EntityCoordinates To) : IWorldCommand
{
    public void Apply(in ParallelApplyContext ctx)
    {
        ctx.Transform.SetCoordinates(Uid, To);
    }
}
