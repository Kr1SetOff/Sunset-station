namespace Content.Shared._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - applied to whatever a Voidwalker is currently pulling: heavily damps the pulled
/// body's velocity so it settles behind the Voidwalker when it stops, instead of endlessly orbiting
/// the loose distance joint ("dancing"). Removed (and the original damping restored) when the pull
/// ends. See VoidwalkerSystem.OnPullStarted/OnPullStopped.
/// </summary>
[RegisterComponent, Access(typeof(SharedVoidwalkerSystem))]
public sealed partial class VoidGripComponent : Component
{
    [DataField]
    public float OldLinearDamping;
}
