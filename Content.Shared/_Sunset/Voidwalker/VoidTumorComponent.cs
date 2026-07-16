using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - marks the organ implanted into a Kidnap victim (see VoidwalkerSystem.OnKidnapDoAfter).
/// While it's still sitting in the victim's body, it slowly darkens them toward full void
/// corruption; get it surgically removed (it's a plain cavity item, so the existing "Extract Item"
/// surgery already finds it - no new surgery steps needed) before EndTime to cure them early.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedVoidwalkerSystem))]
public sealed partial class VoidTumorComponent : Component
{
    /// <summary>
    /// The victim this tumor is growing inside. Kept alongside OrganComponent.Body for convenience,
    /// and networked so the client-side visuals system knows who to darken.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntityUid Victim;

    /// <summary>
    /// When this completes (matches the Kidnap victim's VoidedComponent.EndTime) if never removed.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan EndTime;

    /// <summary>
    /// When this tumor started growing - used to compute how far along the corruption is.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan StartTime;

    /// <summary>
    /// Server-side pacing for the periodic damage tick - not networked, purely internal bookkeeping.
    /// </summary>
    [DataField]
    public TimeSpan NextEffectTime;
}
