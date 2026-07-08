namespace Content.Shared._Sunset.Weapons;

/// <summary>
/// Ported from Goobstation/Reserve-Station as a data-only stub: this fork doesn't have the base
/// stamina-crit "delayed knockdown" pipeline that reads this modifier upstream, so it's currently
/// inert - present so the ported armor entities that reference it still validate and compile.
/// </summary>
[RegisterComponent]
public sealed partial class ModifyDelayedKnockdownComponent : Component
{
    [DataField]
    public bool Cancel;

    [DataField]
    public float DelayDelta;

    [DataField]
    public float KnockdownTimeDelta;
}
