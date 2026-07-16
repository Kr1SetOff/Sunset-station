namespace Content.Shared._Sunset.Chemistry.Components;

/// <summary>
/// 🌇Sunset🌇 - while present, all the entity's fixtures are made non-hard, so it walks straight
/// through walls, doors, windows, and everything else. Granted by the BluespaceDistorter reagent
/// (SunsetBluespacePhase effect), expired and reverted by BluespacePhaseSystem.
/// </summary>
[RegisterComponent]
public sealed partial class BluespacePhaseComponent : Component
{
    [DataField]
    public TimeSpan EndTime;
}
