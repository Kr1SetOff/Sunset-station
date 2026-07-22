using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - porting tgstation's /datum/action/innate/cult/blood_magic (blood_magic.dm): carve
/// unnatural symbols into your own flesh to awaken blood magic. Unlike the rune-based rites (Convert,
/// Summon, etc.) this doesn't need the dagger - it's the cultist's own innate mark, always available.
/// Real tg lets you carve repeatedly to stockpile a hand-picked loadout of up to ~6 spells (chosen from
/// a list each time); simplified here to a single one-time carve that grants a starter kit of three
/// spells at once, since building a second selection UI on top of the dagger's ritual wheel wasn't
/// worth it for a first pass.
/// </summary>
public sealed partial class RatvarCultPrepareBloodMagicActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class RatvarCultPrepareBloodMagicDoAfterEvent : SimpleDoAfterEvent;

/// <summary>
/// 🌇Sunset🌇 - porting /obj/item/melee/blood_magic/stun: empowers the next touch to stun and mute.
/// Simplified to a direct entity-target action instead of a temporary "glowing hand" item.
/// </summary>
public sealed partial class RatvarCultBloodStunActionEvent : EntityTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - porting /datum/action/innate/cult/blood_spell/emp: emits an EMP pulse from your hand.
/// </summary>
public sealed partial class RatvarCultBloodEmpActionEvent : InstantActionEvent;

/// <summary>
/// 🌇Sunset🌇 - porting /datum/action/innate/cult/blood_spell/dagger: summons a replacement ritual
/// dagger in case yours was lost or stolen.
/// </summary>
public sealed partial class RatvarCultBloodDaggerActionEvent : InstantActionEvent;
