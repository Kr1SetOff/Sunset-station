namespace Content.Shared._Sunset.Grab.Components;

/// <summary>
/// Marks a mob that was just thrown out of a grab. While present, the first hard collision with
/// another mob knocks both of them down. Expires on its own shortly after being thrown as a safety net.
/// </summary>
[RegisterComponent]
public sealed partial class GrabThrownComponent : Component
{
    [DataField]
    public EntityUid? Thrower;

    [DataField]
    public TimeSpan SpawnTime;

    [DataField]
    public TimeSpan Lifetime = TimeSpan.FromSeconds(0.5);

    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(4);
}
