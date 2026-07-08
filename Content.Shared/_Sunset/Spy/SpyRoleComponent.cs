using Content.Shared.Roles.Components;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Spy;

/// <summary>
/// Added to mind role entities to tag that they are a Spy - a /tg/station-style Thief antag
/// that steals a set of corporate-espionage-flavored items and escapes with them.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SpyRoleComponent : BaseMindRoleComponent;
