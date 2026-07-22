using Content.Shared.Actions;

namespace Content.Shared._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - scribe a Rune of Offering at the targeted tile. Porting tgstation's actual Rite of
/// Offering (code/modules/antagonists/cult/runes.dm, /obj/effect/rune/convert): the rune itself does
/// nothing until an incapacitated non-cultist is on it and at least 2 living cultists invoke it
/// together (RatvarConvertRuneComponent), rather than a solo dagger-stab.
/// </summary>
public sealed partial class RatvarCultConvertActionEvent : WorldTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - send a telepathic message heard by every other living cultist, wherever they are.
/// </summary>
public sealed partial class RatvarCultCommuneActionEvent : InstantActionEvent;

/// <summary>
/// 🌇Sunset🌇 - scribe the summoning rune at the targeted tile.
/// </summary>
public sealed partial class RatvarCultScribeSummonActionEvent : WorldTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - scribe a Barrier rune at the targeted tile.
/// </summary>
public sealed partial class RatvarCultScribeWallActionEvent : WorldTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - scribe a Boiling Blood rune at the targeted tile.
/// </summary>
public sealed partial class RatvarCultScribeBloodBoilActionEvent : WorldTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - scribe a Teleport rune at the targeted tile.
/// </summary>
public sealed partial class RatvarCultScribeTeleportActionEvent : WorldTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - scribe a Raise Dead rune at the targeted tile.
/// </summary>
public sealed partial class RatvarCultScribeRaiseDeadActionEvent : WorldTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - scribe an Apocalypse rune at the targeted tile.
/// </summary>
public sealed partial class RatvarCultScribeApocalypseActionEvent : WorldTargetActionEvent;
