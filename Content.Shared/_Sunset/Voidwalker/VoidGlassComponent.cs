using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - marks a wall/window turned temporarily passable ("glassed") by a Voidwalker's
/// Glassify ability. Collision is disabled for the duration; a client-side visual system
/// (VoidGlassVisualsSystem) tints the sprite while this is present. See
/// Content.Server._Sunset.Voidwalker.VoidwalkerSystem.OnGlassify.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedVoidwalkerSystem))]
public sealed partial class VoidGlassComponent : Component
{
    [DataField]
    public TimeSpan EndTime;
}
