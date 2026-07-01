namespace Content.Shared._Sunset.MartialArts.Components;

/// <summary>
/// Companion marker for <see cref="Content.Shared.Speech.Muting.MutedComponent"/> that gives it an
/// expiry time, since the base component itself has none.
/// </summary>
[RegisterComponent]
public sealed partial class TemporaryMuteComponent : Component
{
    [DataField]
    public TimeSpan ExpiresAt;
}
