namespace Content.Server._Sunset.MalfAi;

/// <summary>
/// 🌇Sunset🌇 - a machine that a malfunctioning AI has overloaded. Buzzes for a few seconds (the
/// tg-style audible warning) and then explodes; processed by MalfAiSystem.Update.
/// </summary>
[RegisterComponent]
public sealed partial class MalfAiOverloadingComponent : Component
{
    /// <summary>
    /// When the machine detonates.
    /// </summary>
    [DataField]
    public TimeSpan ExplodeAt;

    /// <summary>
    /// The AI that overloaded this machine, for admin logs and explosion attribution.
    /// </summary>
    [DataField]
    public EntityUid? Cause;
}
