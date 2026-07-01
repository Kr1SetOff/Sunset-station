using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Tracks a Sleeping Carp practitioner's self-training ritual and the Gnashing Teeth combo's stacking
/// damage counter. Mastery is trained by re-using the scroll 4 times total (stage 0 -&gt; 1 -&gt; 2 -&gt; 3,
/// then the 4th use grants the style), each requiring a random cooldown before the next use "sticks" -
/// matching Goob Station exactly rather than progressing automatically over time.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SleepingCarpMasteryComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Stage;

    [DataField, AutoNetworkedField]
    public TimeSpan NextStageReady = TimeSpan.Zero;

    [DataField]
    public TimeSpan StageDelayMin = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan StageDelayMax = TimeSpan.FromSeconds(90);

    [DataField]
    public int ConsecutiveGnashes;

    [DataField]
    public TimeSpan LastGnashTime;
}
