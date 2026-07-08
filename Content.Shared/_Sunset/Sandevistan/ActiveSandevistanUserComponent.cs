using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Sandevistan;

/// <summary>
/// Marker component for currently enabled Sandevistan users, optimizing query.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveSandevistanUserComponent : Component;
