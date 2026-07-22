using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - marker for "Conceal Gears" (RU wiki counterplay spell): while present, a Ratvar
/// structure (Altar/Herald's Beacon/Workshop) is hidden client-side - see
/// Content.Client._Sunset.Ratvar.RatvarConcealedVisualsSystem. Toggled by RatvarServantSystem.OnConcealGears.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class RatvarConcealedComponent : Component;
