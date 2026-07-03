// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.GameObjects;

namespace Content.Server._Sunset.Parallel.Commands;

/// <summary>
/// Нанести/снять урон. Сам расчёт «кому и сколько» делается в фоне и кладётся
/// в команду как готовый <see cref="DamageSpecifier"/>; здесь только применение.
///
/// Почему ТОЛЬКО главный поток: TryChangeDamage внутри поднимает события
/// (BeforeDamageChangedEvent и т.п.) и меняет компонент с Dirty — это не
/// потокобезопасно.
///
/// Замечание про аллокации: команды — readonly record struct, но при складывании
/// в List&lt;IWorldCommand&gt; они боксируются. Для прототипа это приемлемо. В горячих
/// путях (атмос) вместо общего буфера нужно использовать типизированный буфер
/// конкретной команды (List&lt;ApplyAtmosphereChange&gt;), чтобы бокса не было.
/// </summary>
public readonly record struct DamageCommand(
    EntityUid Target,
    DamageSpecifier Damage,
    bool IgnoreResistances = false) : IWorldCommand
{
    public void Apply(in ParallelApplyContext ctx)
    {
        ctx.Damageable.TryChangeDamage(Target, Damage, IgnoreResistances);
    }
}
