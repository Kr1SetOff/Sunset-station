// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.NewPlayer;

/// <summary>
/// НОВОЕ. Помечает игрока-новичка: тех, у кого общий налёт на сервере меньше <see cref="Threshold"/>.
/// Серверная система пересчитывает <see cref="IsNewbie"/> и <see cref="Playtime"/> при изменении налёта,
/// клиент по этим (сетевым) полям рисует иконку над головой, а examine показывает наигранные часы.
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class NewPlayerComponent : Component
{
    /// <summary>
    /// Является ли игрок новичком прямо сейчас. Считается на сервере, реплицируется всем клиентам.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField]
    public bool IsNewbie;

    /// <summary>
    /// Общий налёт игрока на сервере. Реплицируется, чтобы examine мог показать часы у любого игрока.
    /// </summary>
    [DataField, ViewVariables(VVAccess.ReadOnly)]
    [AutoNetworkedField]
    public TimeSpan Playtime = TimeSpan.Zero;

    /// <summary>
    /// Порог, ниже которого игрок считается новичком. По умолчанию 16 часов.
    /// </summary>
    [DataField]
    public TimeSpan Threshold = TimeSpan.FromHours(16);

    /// <summary>
    /// Иконка, отображаемая над головой новичка.
    /// </summary>
    [DataField]
    public ProtoId<NewPlayerIconPrototype> Icon = "NewPlayerIcon";
}
