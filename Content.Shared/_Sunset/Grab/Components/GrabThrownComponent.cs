using Content.Shared.FixedPoint;

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

    /// <summary>
    /// How long both the thrown mob and whoever it collides with are knocked down for - same as a
    /// stun, but they can still crawl while down (see <see cref="Content.Shared.Stunnable.SharedStunSystem"/>).
    /// </summary>
    [DataField]
    public TimeSpan KnockdownDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public FixedPoint2 CollisionDamageMin = 10;

    [DataField]
    public FixedPoint2 CollisionDamageMax = 15;
}
