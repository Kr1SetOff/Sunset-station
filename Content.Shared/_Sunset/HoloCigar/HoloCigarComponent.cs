using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._Sunset.HoloCigar;

// 🌇Sunset🌇 - ported from Goobstation/Reserve-Station: a mask-slot holo-cigar. While lit, it plays
// looping music and makes its wearer's dual-wielded guns fire together (see HoloCigarSystem.cs).
// Simplified from upstream: skips the separate per-gun Multishot stat tracking/restoration - the fire-
// together behaviour is gated on the wearer having this lit instead, reusing this fork's existing
// native dual-wield system (Content.Shared._Starlight.Weapons.DualWield) rather than duplicating it.
[RegisterComponent, NetworkedComponent]
public sealed partial class HoloCigarComponent : Component
{
    [ViewVariables]
    public bool Lit;

    [DataField]
    public SoundSpecifier Music = new SoundPathSpecifier(
        "/Audio/_Goobstation/Items/TheManWhoSoldTheWorld/invisibingle.ogg",
        new AudioParams().WithLoop(true).WithVolume(-3f));

    [DataField]
    public SoundSpecifier DeathAudio = new SoundPathSpecifier("/Audio/_Goobstation/Items/TheManWhoSoldTheWorld/ouchies.ogg");

    [ViewVariables]
    public EntityUid? MusicEntity;
}
