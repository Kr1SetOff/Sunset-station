using Content.Shared.Eui;
using Robust.Shared.Serialization;

namespace Content.Shared._Sunset.Discord;

[NetSerializable, Serializable]
public sealed class SunsetDiscordLinkEuiState : EuiStateBase
{
    /// <summary>
    /// The Discord OAuth2 "authorize" URL to open in a browser. Empty if the server hasn't configured OAuth yet.
    /// </summary>
    public string Url { get; set; } = "";

    /// <summary>
    /// True once this account has a linked Discord account (regardless of whether it resolved to an active sponsor tier).
    /// </summary>
    public bool IsLinked { get; set; }

    /// <summary>
    /// The currently resolved sponsor tier, 0 (None) through 5 (Ghost).
    /// </summary>
    public int Tier { get; set; }
}

/// <summary>
/// Sent by the client when the player presses "Refresh status" - re-checks their Discord roles without
/// requiring a full relink.
/// </summary>
[Serializable, NetSerializable]
public sealed class SunsetDiscordRefreshRequestMessage : EuiMessageBase
{
}
