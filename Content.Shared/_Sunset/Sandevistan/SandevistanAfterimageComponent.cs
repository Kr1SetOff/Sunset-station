using Robust.Shared.GameStates;
using Robust.Shared.Maths;

namespace Content.Shared._Sunset.Sandevistan;

/// <summary>
/// Component for afterimage entities spawned by Sandevistan users while active - see
/// SandevistanSystem.SpawnAfterimage. The client-only rendering (copying the source's sprite,
/// tinting it, positioning it) lives in Content.Client._Sunset.Sandevistan.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SandevistanAfterimageComponent : Component
{
    /// <summary>The entity that spawned this afterimage.</summary>
    [DataField, AutoNetworkedField]
    public EntityUid SourceEntity;

    /// <summary>The hue for the rainbow color cycle.</summary>
    [DataField, AutoNetworkedField]
    public float Hue;

    /// <summary>The direction the user's sprite was facing when the afterimage was spawned.</summary>
    [DataField, AutoNetworkedField]
    public Direction DirectionOverride;
}
