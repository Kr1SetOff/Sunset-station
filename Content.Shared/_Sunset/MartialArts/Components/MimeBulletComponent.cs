namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Marks a projectile as a Mime's "Finger Guns" bullet - on hit it mutes the target for
/// <see cref="MuteDuration"/> in addition to whatever damage its ProjectileComponent already deals.
/// </summary>
[RegisterComponent]
public sealed partial class MimeBulletComponent : Component
{
    [DataField]
    public TimeSpan MuteDuration = TimeSpan.FromSeconds(20);
}
