namespace Content.Shared._Sunset.ContractorBaton;

/// <summary>
/// Ported from Goobstation/Reserve-Station: paralyzes any borg struck by this weapon, see
/// <see cref="StunBorgsOnHitSystem"/>.
/// </summary>
[RegisterComponent, Access(typeof(StunBorgsOnHitSystem))]
public sealed partial class StunBorgsOnHitComponent : Component
{
    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(5);
}
