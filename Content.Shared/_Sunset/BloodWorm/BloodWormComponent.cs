using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.BloodWorm;

/// <summary>
/// 🌇Sunset🌇 - a blood worm (ported from tg/station's /mob/living/basic/blood_worm): a Syndicate
/// bioweapon leech that drinks blood to grow through three stages (hatchling - juvenile - adult),
/// can invade and puppet humanoid corpses, spit corrosive blood and, as an adult, lay egg cocoons
/// that hatch into new ghost-role worms. Handled by Content.Server._Sunset.BloodWorm.BloodWormSystem.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class BloodWormComponent : Component
{
    /// <summary>
    /// Total blood units consumed over this worm's whole life. Carried over between growth stages,
    /// like tg's consumed_normal_blood: the juvenile cocoon needs 500 total, the adult one 1500.
    /// Networked so the client can render the blood-counter alert (see BloodWormVisualLayers).
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ConsumedBlood;

    /// <summary>
    /// Total consumed blood required to mature into <see cref="NextStage"/>. Null = final stage.
    /// </summary>
    [DataField]
    public float? MatureThreshold;

    /// <summary>
    /// The mob prototype this worm grows into when maturing. Null = final stage.
    /// </summary>
    [DataField]
    public EntProtoId? NextStage;

    /// <summary>
    /// Cocoon entity the worm hides inside of while maturing - see BloodWormCocoonComponent. The
    /// worm is contained (invisible, non-colliding) for <see cref="CocoonDuration"/>, then the
    /// cocoon is deleted and <see cref="NextStage"/> appears in its place.
    /// </summary>
    [DataField]
    public EntProtoId? CocoonPrototype;

    /// <summary>
    /// How long the worm stays hidden inside the cocoon before emerging as the next stage.
    /// </summary>
    [DataField]
    public TimeSpan CocoonDuration = TimeSpan.FromSeconds(8);

    /// <summary>
    /// How long the maturation channel (curling up, before the cocoon appears) takes.
    /// </summary>
    [DataField]
    public TimeSpan MatureTime = TimeSpan.FromSeconds(8);

    /// <summary>
    /// Blood drained from a victim per completed leech channel.
    /// </summary>
    [DataField]
    public float LeechAmount = 65f;

    /// <summary>
    /// Healing (brute) the worm gets per completed leech, as a fraction of the drained blood.
    /// </summary>
    [DataField]
    public float LeechHealFraction = 0.25f;

    [DataField]
    public TimeSpan LeechTime = TimeSpan.FromSeconds(3);

    /// <summary>
    /// Projectile spat by Spit Blood. Null = this stage can't spit (hatchling).
    /// </summary>
    [DataField]
    public EntProtoId? SpitProjectile;

    /// <summary>
    /// Brute damage the worm deals to itself per spit (tg: costs your own health/blood).
    /// </summary>
    [DataField]
    public float SpitHealthCost = 8f;

    /// <summary>
    /// Egg cocoon prototype laid by Reproduce. Only set on the adult.
    /// </summary>
    [DataField]
    public EntProtoId? EggPrototype;

    /// <summary>
    /// Consumed blood spent per laid egg.
    /// </summary>
    [DataField]
    public float ReproduceCost = 500f;

    /// <summary>
    /// Minimum blood level fraction a corpse needs to be invadable.
    /// </summary>
    [DataField]
    public float InvadeMinBloodLevel = 0.2f;

    [DataField]
    public TimeSpan InvadeTime = TimeSpan.FromSeconds(4);

    /// <summary>
    /// Blood units per second the puppeted host bleeds away while the worm controls it - the worm
    /// literally burns the host's blood to run the body.
    /// </summary>
    [DataField]
    public float HostBloodUpkeep = 2f;

    /// <summary>
    /// Blood level fraction below which the worm is forcibly ejected from its host.
    /// </summary>
    [DataField]
    public float HostEjectBloodLevel = 0.05f;

    /// <summary>
    /// tg's Inject Blood: heals the host's blood/wounds at the cost of the worm's own health.
    /// </summary>
    [DataField]
    public float InjectHealthCost = 15f;

    /// <summary>
    /// Blood restored to the host per Inject Blood use.
    /// </summary>
    [DataField]
    public float InjectBloodRestore = 40f;

    /// <summary>
    /// Brute/burn damage healed on the host per Inject Blood use.
    /// </summary>
    [DataField]
    public float InjectDamageHealed = 10f;

    /// <summary>
    /// tg's Revive Host: restarts a dead host's blood circulation, at a much steeper cost than
    /// Inject Blood since it's bringing someone back from the dead.
    /// </summary>
    [DataField]
    public float ReviveHealthCost = 40f;

    /// <summary>
    /// Blood restored to the host on revival - enough to not immediately die again, not a full heal.
    /// Same units as <see cref="InjectBloodRestore"/> (SharedBloodstreamSystem.TryModifyBloodLevel's
    /// "amount", not a 0-1 fraction - that's what GetBloodLevel returns, a different scale).
    /// </summary>
    [DataField]
    public float ReviveBloodRestore = 60f;

    /// <summary>
    /// The corpse this worm is currently puppeting, if any.
    /// </summary>
    [DataField]
    public EntityUid? Host;
}

/// <summary>
/// 🌇Sunset🌇 - layer keys for the blood-counter alert's digit sprites (see AlertBloodWormBloodSpriteView),
/// mirroring the vampire's blood-drunk counter (Content.Shared._Starlight.Antags.Vampires.VampireVisualLayers).
/// </summary>
public enum BloodWormVisualLayers : byte
{
    Digit1,
    Digit2,
    Digit3,
    Digit4,
}
