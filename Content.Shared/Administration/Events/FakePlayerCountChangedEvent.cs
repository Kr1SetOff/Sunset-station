using Robust.Shared.Serialization;

namespace Content.Shared.Administration.Events;

// 🌇Sunset🌇 - broadcasts the host-configurable "fake player count" padding (see AdminSystem /
// GameTicker.StatusShell.cs) to Host admins so the "Игроки+" tab can show the live value.
[Serializable, NetSerializable]
public sealed class FakePlayerCountChangedEvent : EntityEventArgs
{
    public int Padding;

    public FakePlayerCountChangedEvent(int padding)
    {
        Padding = padding;
    }
}
