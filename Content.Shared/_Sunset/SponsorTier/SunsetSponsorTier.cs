namespace Content.Shared._Sunset.SponsorTier;

/// <summary>
/// The 5 Boosty sponsor tiers, in ascending order of perks. Numeric order matters -
/// tier gating checks (job whitelist, ghost themes, antag weighting) compare with >=.
/// </summary>
public enum SunsetSponsorTier
{
    None = 0,
    Zombie = 1,
    SyndicateAgent = 2,
    Vampire = 3,
    SunSetter = 4,
    Ghost = 5,
}
