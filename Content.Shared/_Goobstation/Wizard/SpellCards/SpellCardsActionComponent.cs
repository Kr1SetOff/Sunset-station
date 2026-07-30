// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Wizard.SpellCards;

[RegisterComponent, NetworkedComponent]
public sealed partial class SpellCardsActionComponent : Component
{
    [ViewVariables(VVAccess.ReadWrite)]
    public int UsesLeft = 6;

    [DataField]
    public int CastAmount = 6;

    [DataField]
    public TimeSpan UseDelay = TimeSpan.FromSeconds(6f);

    [ViewVariables(VVAccess.ReadWrite)]
    public bool PurpleCard = false;
}
