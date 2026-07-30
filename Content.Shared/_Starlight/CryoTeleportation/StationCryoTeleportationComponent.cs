using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._Starlight.CryoTeleportation;

[RegisterComponent]
public sealed partial class StationCryoTeleportationComponent : Component
{
    /// <summary>
    /// How long a player must be gone from their body - while still connected to the game (e.g.
    /// ghosting away and never returning) - before that body is teleported into cryo storage.
    /// </summary>
    [DataField]
    public TimeSpan NotInBodyDelay = TimeSpan.FromMinutes(15);

    /// <summary>
    /// How long a body waits after its player fully disconnects before being teleported into cryo
    /// storage. 🌇Sunset🌇 - raised from 30 seconds to 15 minutes (matching NotInBodyDelay) - a short
    /// debounce was cryoing people almost immediately on a disconnect, punishing brief drops/crashes
    /// instead of just genuine leaves.
    /// </summary>
    [DataField]
    public TimeSpan DisconnectDelay = TimeSpan.FromMinutes(15);

    [DataField]
    public EntProtoId PortalPrototype = "CryoPortal";

    [DataField]
    public SoundSpecifier TransferSound = new SoundPathSpecifier("/Audio/Effects/teleport_departure.ogg");
}
