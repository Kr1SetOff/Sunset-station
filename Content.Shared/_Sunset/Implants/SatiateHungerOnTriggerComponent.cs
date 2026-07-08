using Content.Shared.Trigger.Components.Effects;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.Implants;

/// <summary>
/// Restores the triggering user's hunger by a flat amount - used by NutrimentImplant, following the
/// same activatable-implant pattern as the stock ScramOnTrigger/EmpOnTrigger implants.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SatiateHungerOnTriggerComponent : BaseXOnTriggerComponent
{
    [DataField, AutoNetworkedField]
    public float HungerAmount = 60f;
}
