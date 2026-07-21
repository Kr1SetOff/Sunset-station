using System.Linq;
using Content.Shared.Audio.Jukebox;
using Robust.Client.Audio;
using Robust.Client.UserInterface;
using Robust.Shared.Audio.Components;
using Robust.Shared.Prototypes;

namespace Content.Client._Sunset.Boombox;

/// <summary>
/// 🌇Sunset🌇 - Boombox-only counterpart to JukeboxBoundUserInterface, wired to BoomboxMenu instead
/// of the stationary jukebox machine's plain JukeboxMenu. Message flow is otherwise identical, plus
/// the new loop toggle.
/// </summary>
public sealed partial class BoomboxBoundUserInterface : BoundUserInterface
{
    [Dependency] private IPrototypeManager _protoManager = default!;

    [ViewVariables]
    private BoomboxMenu? _menu;

    public BoomboxBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
        IoCManager.InjectDependencies(this);
    }

    protected override void Open()
    {
        base.Open();

        _menu = this.CreateWindow<BoomboxMenu>();

        _menu.OnPlayPressed += args =>
        {
            if (args)
            {
                SendMessage(new JukeboxPlayingMessage());
            }
            else
            {
                SendMessage(new JukeboxPauseMessage());
            }
        };

        _menu.OnStopPressed += () =>
        {
            SendMessage(new JukeboxStopMessage());
        };

        _menu.OnSongSelected += SelectSong;

        _menu.SetTime += SetTime;
        _menu.OnVolumeChanged += SetVolume;
        _menu.OnLoopChanged += SetLoop;
        PopulateMusic();
        Reload();
    }

    /// <summary>
    /// Reloads the attached menu if it exists.
    /// </summary>
    public void Reload()
    {
        if (_menu == null || !EntMan.TryGetComponent(Owner, out JukeboxComponent? jukebox))
            return;

        _menu.SetAudioStream(jukebox.AudioStream);
        _menu.SetVolume(jukebox.Volume);
        _menu.SetLoop(jukebox.Loop);

        if (_protoManager.Resolve(jukebox.SelectedSongId, out var songProto))
        {
            // A corrupt/unloadable audio file must not blow up Reload() - this runs every time the
            // server resends jukebox state, and an uncaught exception here gets treated as a broken
            // entity state, force-deleting the boombox locally and triggering repeated PVS resyncs.
            var length = TimeSpan.Zero;
            try
            {
                length = EntMan.System<AudioSystem>().GetAudioLength(songProto.Path.Path.ToString());
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to get audio length for boombox song '{songProto.ID}' ({songProto.Path.Path}): {e}");
            }

            _menu.SetSelectedSong(songProto.Name, (float) length.TotalSeconds);
        }
        else
        {
            _menu.SetSelectedSong(string.Empty, 0f);
        }
    }

    public void PopulateMusic()
    {
        if (!EntMan.TryGetComponent(Owner, out JukeboxComponent? jukebox))
            return;

        _menu?.Populate(_protoManager.EnumeratePrototypes<JukeboxPrototype>()
            .Where(song => song.Category == jukebox.Category));
    }

    public void SelectSong(ProtoId<JukeboxPrototype> songid)
    {
        SendMessage(new JukeboxSelectedMessage(songid));
    }

    public void SetTime(float time)
    {
        // See JukeboxBoundUserInterface.SetTime for why this pings-compensates instead of predicting.
        if (EntMan.TryGetComponent(Owner, out JukeboxComponent? jukebox) &&
            EntMan.TryGetComponent(jukebox.AudioStream, out AudioComponent? audioComp))
        {
            audioComp.PlaybackPosition = time;
        }

        SendMessage(new JukeboxSetTimeMessage(time));
    }

    public void SetVolume(float volume)
    {
        SendMessage(new JukeboxSetVolumeMessage(volume));
    }

    public void SetLoop(bool loop)
    {
        SendMessage(new JukeboxSetLoopMessage(loop));
    }
}
