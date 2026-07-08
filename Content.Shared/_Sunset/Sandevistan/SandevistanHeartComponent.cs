namespace Content.Shared._Sunset.Sandevistan;

/// <summary>
/// Marker placed on a heart organ prototype (e.g. OrganHeartStatvekaSandevistan) - when
/// AutosurgeonSystem installs an organ with this marker, it grants SandevistanUserComponent to
/// the recipient (tuned from the fields below) instead of just sitting there as a flat stat bonus.
/// This fork's Organ component has no onAdd component-grant hook like Goobstation's, so the
/// autosurgeon (the only thing that currently installs this heart) does the granting directly.
/// </summary>
[RegisterComponent]
public sealed partial class SandevistanHeartComponent : Component
{
    [DataField]
    public float LoadPerActiveSecond = 1f;

    [DataField]
    public float LoadPerInactiveSecond = -0.25f;

    [DataField]
    public Dictionary<SandevistanState, Content.Shared.FixedPoint.FixedPoint2> Thresholds = new()
    {
        { SandevistanState.Warning, 6 },
        { SandevistanState.Shaking, 10 },
        { SandevistanState.Damage, 20 },
        { SandevistanState.Disable, 24 },
    };

    [DataField]
    public float MovementSpeedModifier = 1.6f;

    [DataField]
    public float AttackSpeedModifier = 1.6f;

    [DataField]
    public bool SlowfieldEnabled = true;
}
