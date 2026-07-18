namespace Content.Shared._Sunset.MalfAi;

/// <summary>
/// 🌇Sunset🌇 - marks an APC that a malfunctioning AI has hacked. Prevents double-hacking the same
/// APC; the crew-visible tell is the emagged screen state the hack applies alongside this marker.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiHackedApcComponent : Component
{
    /// <summary>
    /// The AI brain that hacked this APC.
    /// </summary>
    [DataField]
    public EntityUid? HackedBy;
}
