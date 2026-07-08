using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Autosurgeon;

[Serializable, NetSerializable]
public sealed partial class AutosurgeonDoAfterEvent : SimpleDoAfterEvent;
