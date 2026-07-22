namespace Content.Shared._Sunset.Cult;

/// <summary>
/// 🌇Sunset🌇 - the Barrier rune, porting tgstation's /obj/effect/rune/wall (runes.dm): toggles a
/// temporary wall on/off over the rune each time a cultist interacts with it. Solo-invokable (no
/// cultist-count requirement in tg either).
/// </summary>
[RegisterComponent]
public sealed partial class RatvarWallRuneComponent : Component
{
    /// <summary>
    /// The barrier currently spawned by this rune, if any (null means the barrier is down).
    /// </summary>
    [DataField]
    public EntityUid? Barrier;
}

/// <summary>
/// 🌇Sunset🌇 - the Rite of Boiling Blood, porting tgstation's /obj/effect/rune/blood_boil (runes.dm):
/// requires <see cref="RequiredCultists"/> living cultists nearby to invoke, then burns every
/// non-cultist who can see the rune. Simplified from tg's 3-tick animated burn into a single damage
/// application once the channel completes.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarBloodBoilRuneComponent : Component
{
    [DataField]
    public int RequiredCultists = 3;

    [DataField]
    public float Range = 3f;

    [DataField]
    public TimeSpan ChannelTime = TimeSpan.FromSeconds(3);

    [DataField]
    public bool Channeling;

    /// <summary>
    /// Total burn damage dealt to each non-cultist in range once the rite completes.
    /// </summary>
    [DataField]
    public float Damage = 60f;
}

/// <summary>
/// 🌇Sunset🌇 - a Teleport rune, porting tgstation's /obj/effect/rune/teleport (runes.dm): warps
/// everything standing on it to another linked teleport rune. Real tg lets the invoker pick which
/// linked rune from a list; simplified here to "the nearest other teleport rune" instead of adding a
/// second selection UI on top of the ritual wheel.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarTeleportRuneComponent : Component;

/// <summary>
/// 🌇Sunset🌇 - the Raise Dead rune, porting tgstation's /obj/effect/rune/raise_dead (runes.dm): revives
/// a dead cultist lying on it, spending <see cref="SoulsRequired"/> banked sacrifices (see
/// RatvarCultRuleComponent.SacrificeCount, filled by the Rune of Offering sacrificing instead of
/// converting - RatvarCultSystem.CompleteConvertRite).
/// </summary>
[RegisterComponent]
public sealed partial class RatvarRaiseDeadRuneComponent : Component
{
    [DataField]
    public int SoulsRequired = 3;
}

/// <summary>
/// 🌇Sunset🌇 - the Apocalypse rune, porting tgstation's /obj/effect/rune/apocalypse (runes.dm): a
/// last-resort rite that paralyzes and disorients everyone nearby and unleashes station-wide chaos.
/// Real tg consumes one of a limited set of pre-marked "summoning sites" and forces a chaos event
/// weighted by how outnumbered the cult currently is; simplified here to "paralyze the area + force one
/// random station event" since summon-site gating and the full weighted table are Nar'Sie-specific
/// systems this fork doesn't have.
/// </summary>
[RegisterComponent]
public sealed partial class RatvarApocalypseRuneComponent : Component
{
    [DataField]
    public int RequiredCultists = 3;

    [DataField]
    public float Range = 3f;

    [DataField]
    public TimeSpan ChannelTime = TimeSpan.FromSeconds(10);

    [DataField]
    public bool Channeling;
}
