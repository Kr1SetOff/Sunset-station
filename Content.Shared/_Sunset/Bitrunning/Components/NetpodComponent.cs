using Content.Shared.Roles;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._Sunset.Bitrunning.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NetpodComponent : Component
{
    [DataField]
    public EntityUid? LinkedServer;

    [DataField]
    public EntityUid? Occupant;

    [DataField]
    public EntityUid? Avatar;

    /// <summary>
    /// Internal re-entrancy guard while removing occupant from the pod container.
    /// </summary>
    public bool EjectingOccupant;

    [DataField, AutoNetworkedField]
    public ProtoId<StartingGearPrototype>? PreferredLoadout = "BitrunnerAvatarShaftMinerGear"; // TODO: Replace with BitrunnerGear

    [DataField]
    public List<ProtoId<StartingGearPrototype>> AllowedLoadout = new();

    [DataField]
    public SoundSpecifier OpenSound = new SoundPathSpecifier("/Audio/_Sunset/Bitrunning/tram/tramopen.ogg", AudioParams.Default.WithVolume(-2f).WithVariation(0.1f));

    [DataField]
    public SoundSpecifier CloseSound = new SoundPathSpecifier("/Audio/_Sunset/Bitrunning/tram/tramclose.ogg", AudioParams.Default.WithVolume(-2f).WithVariation(0.1f));

    [DataField]
    public SoundSpecifier ConnectStasisSound = new SoundPathSpecifier("/Audio/_Sunset/Bitrunning/effects/submerge.ogg");

    [DataField]
    public SoundSpecifier ConnectAvatarSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");

    [DataField]
    public SoundSpecifier DisconnectSound = new SoundPathSpecifier("/Audio/Magic/blink.ogg");

    [DataField]
    public SoundSpecifier AutoDisconnectSound = new SoundPathSpecifier("/Audio/_Sunset/Bitrunning/effects/splash.ogg");

    [DataField]
    public SoundSpecifier OccupiedPrySound = new SoundPathSpecifier("/Audio/_Sunset/Bitrunning/airlock/airlock_alien_prying.ogg");

    [DataField]
    public SoundSpecifier OccupiedPryAlertSound = new SoundPathSpecifier("/Audio/_Sunset/Bitrunning/terminal/terminal_alert.ogg");
}
