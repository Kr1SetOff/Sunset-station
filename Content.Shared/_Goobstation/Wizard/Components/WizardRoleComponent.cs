// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Roles;
using Content.Shared.Roles.Components;

namespace Content.Shared._Goobstation.Wizard.Components;

/// <summary>
/// Mind role marker for "you are a wizard". Ported as a direct dependency of the Wizard spell system's
/// BindSoul lich-transformation logic (SpellsSystem.BindSoul checks for this via SharedRoleSystem.MindHasRole).
/// </summary>
[RegisterComponent]
public sealed partial class GoobWizardRoleComponent : BaseMindRoleComponent
{
}
