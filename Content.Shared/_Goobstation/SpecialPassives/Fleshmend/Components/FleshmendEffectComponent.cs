using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._Goobstation.SpecialPassives.Fleshmend.Components;

/// <summary>
///     Component responsible for Fleshmend's visual effects. Should NOT be added outside of FleshmendSystem.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class FleshmendEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public string EffectState = string.Empty;

    [DataField, AutoNetworkedField]
    public ResPath ResPath;
}
