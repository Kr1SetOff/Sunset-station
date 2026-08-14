using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Goobstation.Religion;

/// <summary>
/// DoAfterEvent subclasses must be networked (Shared), even though the systems that raise and
/// handle them (AlternatePrayableSystem, RecallNullrodSystem) are server-only - the client needs
/// to be able to deserialize them for DoAfter prediction/cancellation.
/// </summary>
[Serializable, NetSerializable]
public sealed partial class AlternatePrayDoAfterEvent : SimpleDoAfterEvent;

[Serializable, NetSerializable]
public sealed partial class RecallNullrodDoAfterEvent : SimpleDoAfterEvent;
