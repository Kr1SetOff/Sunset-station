using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Voidwalker;

/// <summary>
/// 🌇Sunset🌇 - marks a Voidwalker ghost role mob (see Resources/Prototypes/_Sunset/Voidwalker).
/// Ported from tg/station's /mob/living/basic/voidwalker, scoped down for a first pass: the mob is
/// stealthy while floating in open space (no grid under it), can dash short distances (reusing the
/// wizard Blink spell), stare-stun ("Unsettle"), send a one-line telepathic message, channel a
/// kidnap on anyone incapacitated while in space (curses them with VoidedComponent instead of
/// actually relocating them to a separate pocket-dimension map), and temporarily turn a nearby wall
/// to passable "glass" (Glassify) so it - and anyone it's pulling - can walk through the opening.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedVoidwalkerSystem))]
public sealed partial class VoidwalkerComponent : Component
{
    /// <summary>
    /// Healing (negative damage) applied every <see cref="SpaceRegenInterval"/> while floating in
    /// open space (no grid under the mob) - the void restores its own.
    /// </summary>
    [DataField]
    public Content.Shared.Damage.DamageSpecifier SpaceRegen = new()
    {
        DamageDict = new()
        {
            { "Blunt", -4 },
            { "Slash", -4 },
            { "Piercing", -4 },
            { "Heat", -3 },
            { "Cold", -3 },
            { "Shock", -3 },
            { "Caustic", -2 },
            { "Radiation", -2 },
        },
    };

    [DataField]
    public TimeSpan SpaceRegenInterval = TimeSpan.FromSeconds(1);

    public TimeSpan NextSpaceRegenTime;

    /// <summary>
    /// Alpha applied while floating in open space (no grid under the mob) - near-invisible.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float SpaceAlpha = 0.15f;

    /// <summary>
    /// Alpha applied everywhere else - fully visible.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float NonSpaceAlpha = 1f;

    /// <summary>
    /// How long a wall stays passable "glass" after Glassify is used on it.
    /// </summary>
    [DataField]
    public TimeSpan GlassifyDuration = TimeSpan.FromSeconds(8);

    [DataField]
    public SoundSpecifier GlassifySound = new SoundPathSpecifier("/Audio/Effects/window_shatter2.ogg");

    /// <summary>
    /// How long it takes to channel a kidnap on an eligible (incapacitated, in-space) victim.
    /// </summary>
    [DataField]
    public TimeSpan KidnapTime = TimeSpan.FromSeconds(6);

    /// <summary>
    /// How long the victim is Voided for after a successful kidnap.
    /// </summary>
    [DataField]
    public TimeSpan VoidedDuration = TimeSpan.FromMinutes(3);

    /// <summary>
    /// Sunset: how long the implanted void tumor takes to fully consume the victim if it's never
    /// cut out. Deliberately much longer than the Voided curse itself, so the crew has a realistic
    /// window to get the victim onto a surgery table.
    /// </summary>
    [DataField]
    public TimeSpan TumorDuration = TimeSpan.FromMinutes(6);

    /// <summary>
    /// Entity spawned on death (see tg's "cosmic skull" loot).
    /// </summary>
    [DataField]
    public EntProtoId DeathLoot = "VoidwalkerCosmicSkull";

    [DataField]
    public SoundSpecifier DeathSound = new SoundPathSpecifier("/Audio/Effects/window_shatter2.ogg");

    /// <summary>
    /// The station this Voidwalker was spawned near (set by VoidwalkerSpawnRuleSystem), used to tell
    /// a ghost which direction to head once they take over. Server-only bookkeeping, not networked.
    /// </summary>
    [DataField]
    public EntityUid? SpawnStation;
}
