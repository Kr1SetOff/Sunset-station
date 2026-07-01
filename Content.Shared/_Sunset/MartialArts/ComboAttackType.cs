namespace Content.Shared._Sunset.MartialArts;

/// <summary>
/// The three inputs that martial arts combos are built out of: a successful unarmed strike (Harm),
/// a successful disarm, and a grab escalation (see Content.Shared._Sunset.Grab).
/// </summary>
public enum ComboAttackType : byte
{
    Harm,
    Disarm,
    Grab,
}
