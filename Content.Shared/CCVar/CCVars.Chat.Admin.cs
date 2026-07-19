using Robust.Shared.Configuration;

namespace Content.Shared.CCVar;

public sealed partial class CCVars
{
    /// <summary>
    ///     The discord channel ID to send admin chat messages to (also receive them). This requires the Discord Integration to be enabled and configured.
    /// </summary>
    public static readonly CVarDef<string> AdminChatDiscordChannelId =
        CVarDef.Create("admin.chat_discord_channel_id", string.Empty, CVar.SERVERONLY);

    /// <summary>
    ///     URL of the Discord webhook which will relay admin chat messages. This is a separate, one-way
    ///     relay that does not require the full Discord bot (token/guild id) to be set up, unlike
    ///     <see cref="AdminChatDiscordChannelId"/>. Point it at a different channel than
    ///     discord.ahelp_webhook so admin chat and ahelp don't end up mixed in the same channel.
    ///     If left empty, disables the webhook.
    /// </summary>
    public static readonly CVarDef<string> AdminChatDiscordWebhook =
        CVarDef.Create("admin.chat_discord_webhook", string.Empty, CVar.SERVERONLY | CVar.CONFIDENTIAL);
}
