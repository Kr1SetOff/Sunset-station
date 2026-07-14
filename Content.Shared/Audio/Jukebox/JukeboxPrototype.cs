using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared.Audio.Jukebox;

/// <summary>
/// Soundtrack that's visible on the jukebox list.
/// </summary>
[Prototype]
public sealed partial class JukeboxPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = string.Empty;

    /// <summary>
    /// User friendly name to use in UI.
    /// </summary>
    [DataField(required: true)]
    public string Name = string.Empty;

    [DataField(required: true)]
    public SoundPathSpecifier Path = default!;

    /// <summary>
    /// 🌇Sunset🌇 - Which jukebox-having entities offer this song (matched against
    /// JukeboxComponent.Category) - lets e.g. the boombox have its own playlist without also
    /// showing up on (or being pickable from) the stationary Jukebox machine, and vice versa.
    /// </summary>
    [DataField]
    public string Category = "Standard";
}
