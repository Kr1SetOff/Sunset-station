using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.Slasher.Components;

/// <summary>
/// Marker component for the Slasher antagonist. Used to apply global slasher rules.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SlasherComponent : Component;
