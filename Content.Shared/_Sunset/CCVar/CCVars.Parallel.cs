// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    /// Аварийный флаг детерминизма для параллельной обработки на базе CommandBuffer.
    /// Если true — все параллельные фазы Sunset выполняются последовательно
    /// (ProcessSerialNow). Должно давать БИТ-В-БИТ тот же результат, что и
    /// параллельный путь; расхождение = баг гонки. Используется в тестах и для
    /// отладки на проде.
    /// </summary>
    public static readonly CVarDef<bool> SunsetParallelDisable =
        CVarDef.Create("sunset.parallel.disable", false, CVar.SERVERONLY);
}
