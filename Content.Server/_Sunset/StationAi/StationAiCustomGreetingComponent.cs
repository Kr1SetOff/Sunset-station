namespace Content.Server._Sunset.StationAi;

/// <summary>
/// 🌇Sunset🌇 - stores the Station AI player's custom arrival-greeting template. The template may
/// use {name} and {job} placeholders; empty/null falls back to the default localized greeting.
/// Lives on the AI "held" (brain) entity, set via the customize-greeting action.
/// </summary>
[RegisterComponent]
public sealed partial class StationAiCustomGreetingComponent : Component
{
    [DataField]
    public string? Greeting;
}
