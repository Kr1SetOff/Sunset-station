using Robust.Shared.GameStates;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom;

namespace Content.Shared._Sunset.Sandevistan;

/// <summary>
/// Marks the local player for the screen-glitch overlay (see SandevistanGlitchOverlay in
/// Content.Client) while the Sandevistan is overloading/disabling - added by
/// SandevistanDisableEffect, cleared automatically once ExpiresAt passes (SandevistanSystem.Update).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentPause]
public sealed partial class SandevistanGlitchComponent : Component
{
    [DataField(customTypeSerializer: typeof(TimeOffsetSerializer)), AutoPausedField]
    public TimeSpan ExpiresAt;
}
