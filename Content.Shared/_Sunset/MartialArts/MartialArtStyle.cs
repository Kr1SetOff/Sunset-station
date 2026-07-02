namespace Content.Shared._Sunset.MartialArts;

public enum MartialArtStyle : byte
{
    None = 0,
    Ninjutsu,
    CQC,
    SleepingCarp,
    Capoeira,

    /// <summary>
    /// The chef's version of CQC - identical moveset to the agent's, but only usable while near a kitchen.
    /// </summary>
    CqcCook,

    /// <summary>
    /// Kung Fu Dragon - rewards standing your ground: staying still for a moment grants a temporary
    /// damage buff on your next attacks.
    /// </summary>
    KungFuDragon,

    /// <summary>
    /// Corporate Judo - throws and holds built around grabbing and downing an opponent. Granted by
    /// wearing a judo belt, not a consumable manual.
    /// </summary>
    CorporateJudo,

    /// <summary>
    /// A Sunset-original style themed around a mime's silent stagecraft - invisible walls, exaggerated
    /// gestures, and the occasional silent scream. Not part of Goob Station's upstream style roster.
    /// </summary>
    Mime,
}
