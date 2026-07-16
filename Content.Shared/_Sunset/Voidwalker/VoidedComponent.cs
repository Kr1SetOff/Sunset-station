using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - applied to a Voidwalker kidnap victim: muted, pacified, and marked for the duration.
/// A simplified stand-in for tg's permanent "Voided" brain trauma (that one requires a whole separate
/// cure item chain to remove) - this one just expires on its own, see SharedVoidwalkerSystem.Update.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedVoidwalkerSystem))]
public sealed partial class VoidedComponent : Component
{
    [DataField]
    public TimeSpan EndTime;
}
