using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Drugs;

/// <summary>
/// 🌇Sunset🌇 - lives on the same status-effect entity as <see cref="SeeingRainbowsStatusEffectComponent"/>.
/// Set (and overwritten, if several hallucinogens are active at once) by <see cref="AddHallucinationTheme"/>
/// each metabolism tick, so the client knows which illusion to spawn for the drug currently driving the
/// trip - see Content.Client.Drugs.HallucinationMobSystem.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class HallucinationMobsComponent : Component
{
    /// <summary>
    /// The illusory mob to spawn client-side while this theme is active. Never actually exists as far
    /// as the server or any other client is concerned.
    /// </summary>
    [DataField(required: true)]
    public EntProtoId Mob;
}
