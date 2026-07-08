// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Sunset.Parallel.Commands;
using Content.Shared.CCVar;
using Content.Shared.Damage.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.GameObjects;
using Robust.Shared.Threading;

namespace Content.Server._Sunset.Parallel;

/// <summary>
/// Фундамент для параллельной обработки по схеме «считаем в фоне → применяем в
/// главном потоке». Сама ничего не делает каждый тик — это переиспользуемый
/// инструмент, который дёргают другие системы (NPC-стиринг, атмос и т.п.).
///
/// Контракт использования:
///   1. Вызывающая система держит свой CommandBuffer (по одному на фазу).
///   2. Перед запуском: buffer.Reset(amount).
///   3. Заполняет поля своего IParallelRobustJob (ссылку на buffer в том числе).
///   4. Зовёт RunAndApply(job, amount, buffer).
///
/// RunAndApply гарантирует БАРЬЕР: к моменту применения команд все фоновые
/// задачи завершены, поэтому гонок между фазой расчёта и фазой применения нет.
/// </summary>
public sealed class ParallelCommandSystem : EntitySystem
{
    [Dependency] private readonly IParallelManager _parallel = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private bool _disableParallel;

    public override void Initialize()
    {
        base.Initialize();
        _cfg.OnValueChanged(CCVars.SunsetParallelDisable, OnDisableChanged, true);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfg.UnsubValueChanged(CCVars.SunsetParallelDisable, OnDisableChanged);
    }

    private void OnDisableChanged(bool value) => _disableParallel = value;

    /// <summary>
    /// Собирает контекст применения. Дёргать только из главного потока.
    /// </summary>
    public ParallelApplyContext CreateContext() => new(EntityManager, _damageable, _transform);

    /// <summary>
    /// Запускает параллельную фазу <paramref name="job"/> на <paramref name="amount"/>
    /// элементов, дожидается её завершения (барьер) и применяет накопленные в
    /// <paramref name="buffer"/> команды в текущем (главном) потоке.
    ///
    /// При взведённом sunset.parallel.disable выполняет фазу последовательно —
    /// результат обязан совпадать с параллельным.
    /// </summary>
    public void RunAndApply(IParallelRobustJob job, int amount, CommandBuffer buffer)
    {
        if (amount > 0)
        {
            // ProcessNow / ProcessSerialNow БЛОКИРУЮТ главный поток до конца всех
            // батчей — это и есть наш барьер перед применением.
            if (_disableParallel)
                _parallel.ProcessSerialNow(job, amount);
            else
                _parallel.ProcessNow(job, amount);
        }

        var ctx = CreateContext();
        buffer.ApplyAll(in ctx);
    }
}
