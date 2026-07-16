using Content.Shared.Actions;

namespace Content.Shared._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - stare at a target until they notice you, stunning and revealing you to them. Ported
/// from tg's /datum/action/cooldown/spell/pointed/unsettle.
/// </summary>
public sealed partial class VoidwalkerUnsettleActionEvent : EntityTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - send a one-line telepathic message to a target. Ported (simplified - fixed phrase
/// pool instead of free text) from tg's "Cosmic Transmit" telepathy action.
/// </summary>
public sealed partial class VoidwalkerTelepathyActionEvent : EntityTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - channel a kidnap on an incapacitated target while in space. Ported (simplified - no
/// separate void pocket-dimension map) from tg's voidwalker kidnap mechanic.
/// </summary>
public sealed partial class VoidwalkerKidnapActionEvent : EntityTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - temporarily turns every wall/window/grille sitting on the targeted tile to passable
/// "glass" at once (so a grille+window pair goes transparent together, not just whichever one
/// happens to be on top), disabling their collision so the Voidwalker - and anyone it's pulling -
/// can simply walk through the opening. New ability, not a tg port - tg's voidwalker phases through
/// walls directly, but this fork has no tile-based movement, so instead of phasing the Voidwalker
/// itself, this opens a temporary hole that normal continuous movement (and the physics pull-joint
/// on a grabbed victim) can walk through. World-targeted (not entity-targeted) specifically so a
/// stacked grille+window tile can be resolved as a whole instead of picking one entity under the cursor.
/// </summary>
public sealed partial class VoidwalkerGlassifyActionEvent : WorldTargetActionEvent;
