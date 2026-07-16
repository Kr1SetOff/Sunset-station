using Content.Shared.Chat.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Emotes;

/// <summary>
/// 🌇Sunset🌇 - lets an entity play the Sunset animated emotes (jump/spin/dance/flip/double flip).
/// The server stamps the triggered emote id here (AnimatedEmotesSystem, server) and the client
/// plays the matching sprite animation on state application (AnimatedEmotesSystem, client).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class AnimatedEmotesComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype>? Emote;

    /// <summary>
    /// Bumped on every trigger so repeating the same emote still produces a fresh component state.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int Counter;
}
