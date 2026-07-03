// SPDX-License-Identifier: AGPL-3.0-or-later

using System;
using System.Collections.Generic;
using Content.Server._Sunset.Parallel.Commands;

namespace Content.Server._Sunset.Parallel;

/// <summary>
/// Потокобезопасный буфер команд для одной параллельной фазы.
///
/// Модель безопасности — ровно как results[] в PathfindingSystem:
/// буфер индексируется по ИНДЕКСУ ЗАДАЧИ (тому самому index из
/// IParallelRobustJob.Execute(int index)). ParallelManager разбивает диапазон
/// 0..amount на непересекающиеся батчи и раздаёт их разным потокам, поэтому
/// два потока ФИЗИЧЕСКИ не могут писать в один и тот же слот _perItem[index].
/// Никаких локов не нужно.
///
/// Слияние и применение (<see cref="ApplyAll"/>) делает главный поток после
/// барьера, последовательно и детерминированно.
/// </summary>
public sealed class CommandBuffer
{
    // Один список команд на каждый индекс задачи. Переиспользуется между тиками,
    // чтобы не давить на GC: Reset() только чистит списки, не пересоздаёт их.
    private List<IWorldCommand>[] _perItem = Array.Empty<List<IWorldCommand>>();
    private int _count;

    /// <summary>
    /// Подготовить буфер под <paramref name="amount"/> задач. Вызывается в главном
    /// потоке ПЕРЕД запуском параллельной фазы. Растит массив при необходимости и
    /// чистит используемые слоты.
    /// </summary>
    public void Reset(int amount)
    {
        if (_perItem.Length < amount)
        {
            var old = _perItem.Length;
            Array.Resize(ref _perItem, amount);
            for (var i = old; i < amount; i++)
                _perItem[i] = new List<IWorldCommand>();
        }

        for (var i = 0; i < amount; i++)
            _perItem[i].Clear();

        _count = amount;
    }

    /// <summary>
    /// Положить команду от задачи <paramref name="index"/>. Вызывается из фонового
    /// потока. Безопасно без локов: index эксклюзивно принадлежит текущему потоку.
    /// </summary>
    public void Add(int index, IWorldCommand command)
    {
        _perItem[index].Add(command);
    }

    /// <summary>
    /// Применить все команды к миру. Вызывается СТРОГО в главном потоке.
    ///
    /// Порядок детерминирован: задачи по возрастанию индекса, команды внутри
    /// задачи — в порядке добавления. Это важно для реплеев и для случаев, когда
    /// две команды бьют по одной цели.
    /// </summary>
    public void ApplyAll(in ParallelApplyContext ctx)
    {
        for (var i = 0; i < _count; i++)
        {
            var list = _perItem[i];
            for (var j = 0; j < list.Count; j++)
                list[j].Apply(in ctx);
        }
    }

    /// <summary>Сколько всего команд накопилось (для метрик/тестов).</summary>
    public int CountCommands()
    {
        var total = 0;
        for (var i = 0; i < _count; i++)
            total += _perItem[i].Count;
        return total;
    }
}
