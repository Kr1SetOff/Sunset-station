namespace Content.Shared._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - the Rune of Offering, porting tgstation's /obj/effect/rune/convert
/// (code/modules/antagonists/cult/runes.dm) - a cultist interacting with the rune starts the rite
/// (Content.Server._Sunset.Cult.RatvarCultSystem.OnConvertRuneInteractHand) only if an incapacitated
/// non-cultist is standing on it; the rite then completes if at least <see cref="RequiredCultists"/>
/// living cultists stay within <see cref="Range"/> and the victim is still there/incapacitated for the
/// whole <see cref="ChannelTime"/> - real tg requires 2 invokers minimum for this specific rune (most
/// other runes only need 1), which is what actually distinguishes conversion from everything else a
/// lone cultist can do solo.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarConvertRuneComponent : Component
{
    [DataField]
    public int RequiredCultists = 2;

    [DataField]
    public float Range = 1.5f;

    [DataField]
    public TimeSpan ChannelTime = TimeSpan.FromSeconds(6);

    /// <summary>
    /// Set once the rite is underway so a second cultist can't start a duplicate channel on the same rune.
    /// </summary>
    [DataField]
    public bool Channeling;

    /// <summary>
    /// The victim currently pinned to this rune for the duration of the rite.
    /// </summary>
    [DataField]
    public EntityUid? Victim;
}
