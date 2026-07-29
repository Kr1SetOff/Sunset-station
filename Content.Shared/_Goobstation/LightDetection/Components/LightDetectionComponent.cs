using Robust.Shared.GameStates;

namespace Content.Shared._Goobstation.LightDetection.Components;

// TODO: Only the bare component is ported here (referenced by Changeling's Darkness Adaption ability
// to tag/detect whether the changeling is standing in light). Goob-Station's LightDetectionSystem
// (Content.Goobstation.Server/Shared/Client/LightDetection) which actually computes CurrentLightLevel
// from nearby light sources was not ported, since nothing in the Changeling port references it directly.
// Someone doing broader system integration would need to port that system for this component's
// CurrentLightLevel to update in real time.

/// <summary>
/// This is used for detecting if an entity is near a lighted area
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(false, true)]
public sealed partial class LightDetectionComponent : Component
{
    /// <summary>
    /// Current light level that entity gets from all light sources in radius
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public float CurrentLightLevel;

    /// <summary>
    /// Minimum light level for entity to be on light
    /// </summary>
    [DataField]
    public float OnLightLevel = 0.25f;

    public bool OnLight => CurrentLightLevel > OnLightLevel;
}
