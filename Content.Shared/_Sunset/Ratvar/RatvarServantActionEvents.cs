using Content.Shared.Actions;

namespace Content.Shared._Sunset.Ratvar;

/// <summary>
/// 🌇Sunset🌇 - the clockwork slab's built-in Stun ability - touch range, stuns and mutes the target.
/// Unlike Nar'Sie's blood magic, this is part of the slab's base kit, not something carved/prepared.
/// </summary>
public sealed partial class RatvarSlabStunActionEvent : EntityTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - the clockwork slab's built-in Teleport ability - short-range blink.
/// </summary>
public sealed partial class RatvarSlabTeleportActionEvent : WorldTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - the clockwork slab's built-in Heal ability - mends a servant (or yourself) standing nearby.
/// </summary>
public sealed partial class RatvarSlabHealActionEvent : EntityTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - Hand of Midas, porting the ss220.space RU wiki's "Рука Мидаса": converts a touched stack
/// of steel or plasteel into brass sheets, 1:1.
/// </summary>
public sealed partial class RatvarHandOfMidasActionEvent : EntityTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - places a Herald's Beacon at the targeted tile, consuming 6 brass sheets from the
/// servant's active hand (RU wiki: "маяк - 6 листов латуни").
/// </summary>
public sealed partial class RatvarPlaceBeaconActionEvent : WorldTargetActionEvent;

/// <summary>
/// 🌇Sunset🌇 - craft a piece of enchanted gear (see RatvarCraftGearComponent on the action prototype
/// for which item/cost). Shared across every craftable item instead of one event class each.
/// </summary>
public sealed partial class RatvarCraftGearActionEvent : InstantActionEvent;

/// <summary>
/// 🌇Sunset🌇 - animate a clockwork construct ally (Cogscarab/Marauder/Brass Golem). Reuses
/// RatvarCraftGearComponent for cost/result data - the action just spawns a mob instead of an item.
/// </summary>
public sealed partial class RatvarCraftConstructActionEvent : InstantActionEvent;

/// <summary>
/// 🌇Sunset🌇 - "Conceal Gears" (RU wiki counterplay spell): toggles RatvarConcealedComponent on a
/// targeted Ratvar structure (Altar/Herald's Beacon/Workshop), hiding it from view client-side.
/// </summary>
public sealed partial class RatvarConcealGearsActionEvent : EntityTargetActionEvent;
