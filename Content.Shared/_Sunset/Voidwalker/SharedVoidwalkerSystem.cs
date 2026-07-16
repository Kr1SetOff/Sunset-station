using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - shared base, split from Content.Server._Sunset.Voidwalker.VoidwalkerSystem purely so
/// VoidwalkerComponent/VoidedComponent can restrict Access to "either half of this system" without
/// depending on the server assembly from Content.Shared.
/// </summary>
public abstract class SharedVoidwalkerSystem : EntitySystem;

[Serializable, NetSerializable]
public sealed partial class VoidwalkerKidnapDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class VoidwalkerUnsettleDoAfterEvent : SimpleDoAfterEvent;
