using Content.Shared._Sunset.MartialArts;

namespace Content.Shared._Sunset.Implants;

/// <summary>
/// Grants a martial arts style the moment this implant is installed. Used for KravMagaImplanter -
/// Goobstation's Krav Maga is its own separate, incompatible martial-arts framework, so this reuses
/// Sunset's existing CQC style instead of porting a second one for a single implant.
/// </summary>
[RegisterComponent, Access(typeof(MartialArtImplantSystem))]
public sealed partial class MartialArtImplantComponent : Component
{
    [DataField(required: true)]
    public MartialArtStyle Style;
}
