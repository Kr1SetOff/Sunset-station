using Content.Shared.Actions;

namespace Content.Shared._Sunset.MartialArts.Events;

/// <summary>
/// Raised when a Mime martial artist uses their Finger Guns action - a real targeted ranged attack
/// (matching Goob Station/Reserve-Station's ActionFingerGuns) that fires a silent mimed bullet,
/// replacing this fork's earlier melee-combo-only substitute.
/// </summary>
public sealed partial class MimeFingerGunsActionEvent : WorldTargetActionEvent;
