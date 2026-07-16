namespace Content.Shared._Sunset.Chemistry.Components;

/// <summary>
/// 🌇Sunset🌇 - a burning star-shaped aura: every pulse, everything flammable within two tiles in
/// each cardinal direction (a plus/star pattern) is set on fire, and the carrier slowly cooks too
/// (~50 burn damage over the full 15 minutes). Granted by the Zapalm reagent (SunsetZapalmAura
/// effect), driven by ZapalmAuraSystem.
/// </summary>
[RegisterComponent]
public sealed partial class ZapalmAuraComponent : Component
{
    [DataField]
    public TimeSpan EndTime;

    [DataField]
    public TimeSpan NextPulse;
}
