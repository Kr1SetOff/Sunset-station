// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._Goobstation.Changeling.Objectives.Systems;

namespace Content.Server._Goobstation.Changeling.Objectives.Components;

[RegisterComponent, Access(typeof(ChangelingObjectiveSystem), typeof(ChangelingSystem))]
public sealed partial class StealDNAConditionComponent : Component
{
    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public float DNAStolen = 0f;
}
