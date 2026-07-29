using Content.Shared.Roles.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Changeling.Components;

/// <summary>
/// Added to mind role entities to tag that they are a changeling.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ChangelingRoleComponent : BaseMindRoleComponent;
