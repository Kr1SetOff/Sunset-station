using Robust.Shared.GameStates;

namespace Content.Shared.Audio.Jukebox;

/// <summary>
/// 🌇Sunset🌇 - marks an audio-stream entity as belonging to a boombox's own playlist (as opposed to
/// the stationary Jukebox machine's), so BoomboxMuteSystem knows which currently-playing sounds to
/// silence for a client that's toggled "audio.mute_boomboxes" - see JukeboxSystem.OnJukeboxPlay
/// (server) and BoomboxMuteSystem (client).
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class BoomboxAudioComponent : Component;
