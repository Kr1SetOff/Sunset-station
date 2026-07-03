// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;
using Robust.Shared.Network;

namespace Content.Shared._Sunset.Biocode;

/// <summary>
/// НОВАЯ МЕХАНИКА «Биокод». Привязывает предмет к одному игроку прямо во время игры
/// (через ПКМ-верб «Установить биокод»). Пока биокод не установлен (<see cref="OwnerUserId"/> == null),
/// предмет общедоступен. После установки:
///  - предметами-инструментами (аплинк, оружие, снаряжение) может пользоваться только владелец
///    (блокируются открытие UI, использование в руке, стрельба);
///  - предметы-скафандры (<see cref="DetonateOnForeignWear"/> == true), надетые ЧУЖИМ игроком,
///    нельзя снять, и скафандр запускает обратный отсчёт с озвучкой в чат, после чего взрывается.
///
/// Компонент в Shared (чтобы прототипы с ним грузились и на клиенте), но вся логика — серверная
/// (см. <c>Content.Server._Sunset.Biocode.BiocodeSystem</c>). Сетевое состояние не требуется:
/// привязка и детонация полностью обрабатываются на сервере.
/// </summary>
[RegisterComponent]
public sealed partial class BiocodeComponent : Component
{
    /// <summary>
    /// Владелец биокода (UserId сессии). null — биокод ещё не установлен, предмет общедоступен.
    /// Устанавливается в рантайме вербом, поэтому не сериализуется из YAML.
    /// </summary>
    public NetUserId? OwnerUserId;

    /// <summary>
    /// Имя владельца на момент установки (для сообщений).
    /// </summary>
    public string OwnerName = string.Empty;

    /// <summary>
    /// Должен ли скафандр взрываться, если его надел чужой игрок. Для аплинка/оружия — false.
    /// </summary>
    [DataField]
    public bool DetonateOnForeignWear = true;

    /// <summary>
    /// Задержка до взрыва скафандра (секунды), когда его надел чужак. ~5–10с.
    /// </summary>
    [DataField]
    public float DetonationDelay = 8f;

    /// <summary>
    /// Параметры взрыва скафандра.
    /// </summary>
    [DataField]
    public float ExplosionTotalIntensity = 40f;

    [DataField]
    public float ExplosionSlope = 3f;

    [DataField]
    public float ExplosionMaxTileIntensity = 8f;

    /// <summary>
    /// Звук установки биокода.
    /// </summary>
    [DataField]
    public SoundSpecifier InstallSound = new SoundPathSpecifier("/Audio/Machines/beep.ogg");

    // --- Рантайм-состояние детонации скафандра ---

    /// <summary>Идёт ли сейчас обратный отсчёт самоуничтожения.</summary>
    public bool Detonating;

    /// <summary>Сколько секунд осталось до взрыва.</summary>
    public float DetonationTimer;

    /// <summary>Аккумулятор для ежесекундной озвучки отсчёта.</summary>
    public float AnnounceAccumulator;

    /// <summary>Последняя озвученная секунда (чтобы не повторять).</summary>
    public int LastAnnounced = -1;

    /// <summary>Кто заперт в скафандре (чужой носитель).</summary>
    public EntityUid? TrappedWearer;
}
