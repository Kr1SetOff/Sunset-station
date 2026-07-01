using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Tracks a martial artist's recent attack inputs so combo sequences (see SharedMartialArtsSystem)
/// can be pattern-matched against them. History resets if <see cref="ResetWindow"/> passes without
/// a new attack, or if the attacker switches targets. Networked so the client can render a small
/// "recent inputs" indicator near the cursor, similar to Goob Station's combo tracker.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CanPerformComboComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ComboAttackType> LastAttacks = new();

    [DataField]
    public EntityUid? CurrentTarget;

    [DataField, AutoNetworkedField]
    public TimeSpan LastAttackTime;

    [DataField, AutoNetworkedField]
    public TimeSpan ResetWindow = TimeSpan.FromSeconds(5);

    [DataField]
    public int LastAttacksLimit = 5;
}
