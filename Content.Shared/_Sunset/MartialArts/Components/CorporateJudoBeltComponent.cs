namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Marker for the corporate judo belt - identifies it for the weapon-lock check in
/// SharedMartialArtsSystem.CorporateJudo.cs without needing a reactive equip-triggered component add.
/// </summary>
[RegisterComponent]
public sealed partial class CorporateJudoBeltComponent : Component;
