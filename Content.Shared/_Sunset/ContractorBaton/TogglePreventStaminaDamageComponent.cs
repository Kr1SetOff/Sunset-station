namespace Content.Shared._Sunset.ContractorBaton;

/// <summary>
/// Ported from Goobstation/Reserve-Station: while the item this is on is deactivated, it deals no
/// stamina damage (used by ContractorBaton's telescopic-baton toggle).
/// </summary>
[RegisterComponent]
public sealed partial class TogglePreventStaminaDamageComponent : Component
{
}
