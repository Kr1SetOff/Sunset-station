using Content.Shared.Silicons.Laws;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.CustomLawboard;

// 🌇Sunset🌇 - ported from Goobstation/Reserve-Station: an item that lets a player write a custom set
// of AI laws and upload them, reusing this fork's existing Content.Shared.Silicons.Laws substrate.
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CustomLawboardComponent : Component
{
    /// <summary>
    /// The laws currently written on this lawboard.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<SiliconLaw> Laws = new();
}
