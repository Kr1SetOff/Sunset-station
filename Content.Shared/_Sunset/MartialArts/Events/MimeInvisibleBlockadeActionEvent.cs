using Content.Shared.Actions;

namespace Content.Shared._Sunset.MartialArts.Events;

/// <summary>
/// Raised when a Mime martial artist uses their Invisible Blockade action - the cooldown-gated
/// counterpart to Goob Station's real ActionInvisibleBlockade, ported as a clickable action instead of
/// a combo trigger so it behaves like the original instead of needing a specific attack sequence.
/// </summary>
public sealed partial class MimeInvisibleBlockadeActionEvent : InstantActionEvent;
