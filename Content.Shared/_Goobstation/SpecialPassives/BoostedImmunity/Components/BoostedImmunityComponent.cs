using Content.Shared.Alert;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Goobstation.SpecialPassives.BoostedImmunity.Components;

/// <summary>
///     Entities with this rapidly cleanse chemicals/toxins and shrug off minor status effects.
///     Simplified from Goob-Station's version, which relies on a Disease system and Xenomorph
///     infection removal that don't exist in this fork.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BoostedImmunityComponent : Component
{
    public ProtoId<AlertPrototype>? AlertId;

    [DataField]
    public float? Duration;

    public TimeSpan MaxDuration = TimeSpan.Zero;

    public TimeSpan UpdateTimer = default!;

    public TimeSpan UpdateDelay = TimeSpan.FromSeconds(1);

    public MobState Mobstate;

    [DataField]
    public bool WorkWhileDead;

    [DataField]
    public bool CleanseChemicals = true;

    [DataField]
    public FixedPoint2 CleanseChemicalsAmount = 25;

    [DataField]
    public bool ApplySober = true;

    [DataField]
    public bool RemovePacifism = true;

    [DataField]
    public float ToxinHeal = -10f;

    [DataField]
    public int EyeDamageHeal = 1;
}
