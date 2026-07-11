namespace Content.Client._Sunset.CustomEmotes;

// 🌇Sunset🌇 Client-saved custom emote preset. Persisted locally as JSON, never sent to the server as data -
// only the resulting "me" chat text is sent when fired, same as if the player typed it by hand.
public sealed class CustomEmoteEntry
{
    public string Name { get; set; } = string.Empty;
    public string Text { get; set; } = string.Empty;
}
