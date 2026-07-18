using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.BloodWorm;

/// <summary>
/// 🌇Sunset🌇 - action events for the blood worm. Handlers in
/// Content.Server._Sunset.BloodWorm.BloodWormSystem.
/// </summary>

/// <summary>Leech Blood - latch onto a living creature and drain its blood.</summary>
public sealed partial class BloodWormLeechEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class BloodWormLeechDoAfterEvent : SimpleDoAfterEvent;

/// <summary>Mature - cocoon up and grow into the next stage (needs enough consumed blood).</summary>
public sealed partial class BloodWormMatureEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class BloodWormMatureDoAfterEvent : SimpleDoAfterEvent;

/// <summary>Spit Blood - launch a glob of corrosive blood, at the cost of the worm's own health.</summary>
public sealed partial class BloodWormSpitEvent : WorldTargetActionEvent;

/// <summary>Invade Corpse - crawl into a humanoid corpse and puppet it.</summary>
public sealed partial class BloodWormInvadeEvent : EntityTargetActionEvent;

[Serializable, NetSerializable]
public sealed partial class BloodWormInvadeDoAfterEvent : SimpleDoAfterEvent;

/// <summary>Leave Host - abandon the puppeted corpse (granted to the host body while possessed).</summary>
public sealed partial class BloodWormLeaveHostEvent : InstantActionEvent;

/// <summary>Inject Blood - heal the host's wounds/blood at the cost of the worm's own health.</summary>
public sealed partial class BloodWormInjectEvent : InstantActionEvent;

/// <summary>Revive Host - restart a dead host's blood circulation, bringing them back to life.</summary>
public sealed partial class BloodWormReviveHostEvent : InstantActionEvent;

/// <summary>Reproduce - the adult lays an egg cocoon that hatches into a ghost-role hatchling.</summary>
public sealed partial class BloodWormReproduceEvent : InstantActionEvent;
