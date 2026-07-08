namespace Content.Shared._Sunset.HoloCigar;

/// <summary>
/// Applied to the wearer of a lit <see cref="HoloCigarComponent"/> - marks them as an unstoppable
/// gunslinger, so their dual-wielded guns fire together (see HoloCigarSystem.cs).
/// </summary>
[RegisterComponent]
public sealed partial class HoloCigarWearerComponent : Component
{
    /// <summary>
    /// The lit cigar granting this. Used to stop its music if the wearer dies.
    /// </summary>
    [ViewVariables]
    public EntityUid HoloCigarEntity;

    /// <summary>
    /// Re-entrancy guard so firing the second gun doesn't try to fire the first one again.
    /// </summary>
    [ViewVariables]
    public bool Firing;
}
