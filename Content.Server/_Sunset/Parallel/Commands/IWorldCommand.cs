// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage.Systems;
using Robust.Shared.GameObjects;

namespace Content.Server._Sunset.Parallel.Commands;

/// <summary>
/// Описание изменения мира, посчитанного в фоновом потоке.
/// Команда — это ТОЛЬКО данные. Любая логика, которая трогает мир
/// (события, спавны, перемещения, Dirty), живёт в <see cref="Apply"/>,
/// и <see cref="Apply"/> вызывается СТРОГО в главном потоке.
///
/// Фоновый поток имеет право только создать команду и положить её в
/// <see cref="CommandBuffer"/>. Он не имеет права вызывать Apply сам.
/// </summary>
public interface IWorldCommand
{
    /// <summary>
    /// Применяет изменение к миру. Вызывается главным потоком после того,
    /// как все фоновые задачи гарантированно завершились (барьер в
    /// <see cref="ParallelCommandSystem.RunAndApply"/>).
    /// </summary>
    void Apply(in ParallelApplyContext ctx);
}

/// <summary>
/// Набор сервисов, нужных командам для применения. Резолвится один раз
/// в <see cref="ParallelCommandSystem"/> и передаётся по ссылке (readonly struct),
/// чтобы не дёргать IoC из каждой команды.
///
/// ВАЖНО: всё, что здесь лежит, используется только в главном потоке.
/// </summary>
public readonly struct ParallelApplyContext
{
    public readonly IEntityManager EntityManager;
    public readonly DamageableSystem Damageable;
    public readonly SharedTransformSystem Transform;

    public ParallelApplyContext(
        IEntityManager entityManager,
        DamageableSystem damageable,
        SharedTransformSystem transform)
    {
        EntityManager = entityManager;
        Damageable = damageable;
        Transform = transform;
    }
}
