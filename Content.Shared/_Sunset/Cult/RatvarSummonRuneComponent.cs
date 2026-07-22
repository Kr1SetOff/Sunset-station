namespace Content.Shared._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - the final summoning rune. An interacting cultist starts the rite
/// (Content.Server._Sunset.Cult.RatvarCultSystem.OnRuneInteractHand); it completes only if at least
/// <see cref="RequiredCultists"/> living cultists stay within <see cref="Range"/> tiles of the rune for
/// the whole <see cref="ChannelTime"/> duration, mirroring the classic "9 cultists on the summoning
/// rune" requirement (scaled down - see the rule component for why).
/// </summary>
[RegisterComponent]
public sealed partial class RatvarSummonRuneComponent : Component
{
    [DataField]
    public int RequiredCultists = 3;

    [DataField]
    public float Range = 2.5f;

    [DataField]
    public TimeSpan ChannelTime = TimeSpan.FromSeconds(10);

    /// <summary>
    /// Set once the rite is underway so a second cultist can't start a duplicate channel on the same rune.
    /// </summary>
    [DataField]
    public bool Channeling;
}
