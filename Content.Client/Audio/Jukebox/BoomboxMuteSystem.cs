using Content.Shared.Audio.Jukebox;
using Content.Shared.CCVar;
using Robust.Shared.Audio.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.GameStates;
using AudioSystem = Robust.Client.Audio.AudioSystem;

namespace Content.Client.Audio.Jukebox;

/// <summary>
/// 🌇Sunset🌇 - lets a client silence every boombox (see BoomboxAudioComponent) for themselves via
/// the "audio.mute_boomboxes" option, without touching the actual networked volume other players
/// hear.
///
/// AudioComponent.Volume and .Gain are NOT independent - Volume's setter just converts dB to gain
/// and writes the same underlying value Gain does (see BaseAudioSource.Volume/Gain in the engine).
/// Robust's own AudioSystem recalculates occlusion (the "muffled through a wall" lowpass filter
/// effect) and re-derives Gain from it EVERY RENDERED FRAME, in its own FrameUpdate - not just on
/// state (re)sync. Re-asserting the mute from EntitySystem.Update() (the simulation TICK, a
/// different and slower cadence than render frames) meant there were frames where the engine's own
/// occlusion recalculation had already put a small non-zero gain back before the next tick corrected
/// it - inaudible as "volume", but the occlusion lowpass filter still let some of that near-silent
/// signal's bass through, which is exactly the "deafened by bass instead of silent" symptom this was
/// causing. Overriding FrameUpdate instead matches the engine's own per-frame cadence exactly, and
/// UpdatesAfter forces our write to run after AudioSystem's for that same frame so it's always the
/// last word.
///
/// Can't just re-subscribe to (AudioComponent, AfterAutoHandleStateEvent) ourselves to reapply it
/// afterwards - Robust only allows ONE directed subscriber per (component, event) pair system-wide,
/// full stop, regardless of before/after ordering, and Robust.Client.Audio.AudioSystem already owns
/// that slot (crashes the client with "Duplicate Subscriptions" on connect otherwise, exactly like
/// the earlier GrabberComponent/ComponentShutdown collision this fork already hit once).
/// </summary>
public sealed class BoomboxMuteSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    private bool _muted;

    public override void Initialize()
    {
        base.Initialize();

        UpdatesAfter.Add(typeof(AudioSystem));

        SubscribeLocalEvent<BoomboxAudioComponent, ComponentStartup>(OnBoomboxAudioStartup);
        _cfg.OnValueChanged(CCVars.MuteBoomboxes, OnMutedChanged, true);
    }

    public override void Shutdown()
    {
        base.Shutdown();
        _cfg.UnsubValueChanged(CCVars.MuteBoomboxes, OnMutedChanged);
    }

    public override void FrameUpdate(float frameTime)
    {
        base.FrameUpdate(frameTime);

        if (!_muted)
            return;

        var query = EntityQueryEnumerator<BoomboxAudioComponent, AudioComponent>();
        while (query.MoveNext(out _, out _, out var audio))
        {
            audio.Gain = 0f;
        }
    }

    private void OnBoomboxAudioStartup(Entity<BoomboxAudioComponent> ent, ref ComponentStartup args)
    {
        if (_muted && TryComp<AudioComponent>(ent.Owner, out var audio))
            audio.Gain = 0f;
    }

    private void OnMutedChanged(bool muted)
    {
        _muted = muted;

        var query = EntityQueryEnumerator<BoomboxAudioComponent, AudioComponent>();
        while (query.MoveNext(out _, out _, out var audio))
        {
            audio.Gain = muted ? 0f : SharedAudioSystem.VolumeToGain(audio.Params.Volume);
        }
    }
}
