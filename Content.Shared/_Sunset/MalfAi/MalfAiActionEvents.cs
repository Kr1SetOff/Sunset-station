using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.MalfAi;

/// <summary>
/// 🌇Sunset🌇 - action events for the Malfunctioning AI's abilities. The free innate actions (open
/// module store, hack APC) and every purchasable module are handled server-side in
/// Content.Server._Sunset.MalfAi.MalfAiSystem.
/// </summary>

/// <summary>Opens the malf module store (CPU shop).</summary>
public sealed partial class MalfAiOpenModulesEvent : InstantActionEvent;

/// <summary>Starts hacking the targeted APC.</summary>
public sealed partial class MalfAiHackApcActionEvent : EntityTargetActionEvent;

/// <summary>Fired when the APC hack do-after completes.</summary>
[Serializable, NetSerializable]
public sealed partial class MalfAiHackApcDoAfterEvent : SimpleDoAfterEvent;

/// <summary>Arms the doomsday device (tg: 450s to disintegration of all station organics).</summary>
public sealed partial class MalfAiDoomsdayEvent : InstantActionEvent;

/// <summary>Hostile Station Lockdown - bolts every airlock on the station for a while.</summary>
public sealed partial class MalfAiLockdownEvent : InstantActionEvent;

/// <summary>Blackout - blows a chunk of the station's light bulbs.</summary>
public sealed partial class MalfAiBlackoutEvent : InstantActionEvent;

/// <summary>Destroy RCDs - detonates every RCD on the station.</summary>
public sealed partial class MalfAiDestroyRcdsEvent : InstantActionEvent;

/// <summary>Machine Overload - overheats the targeted machine into a small explosion.</summary>
public sealed partial class MalfAiOverloadMachineEvent : EntityTargetActionEvent;

/// <summary>Targeted Safeties Override - remotely emags the targeted device.</summary>
public sealed partial class MalfAiOverrideSafetiesEvent : EntityTargetActionEvent;
