using System.Diagnostics.CodeAnalysis;
using System.Text;
using Content.Shared._Sunset.SponsorTier;
using Robust.Shared.Player;
using Robust.Shared.Utility;

namespace Content.Server.Chat.Managers;

// 🌇Sunset🌇 - Boosty sponsor tier + admin bracket tags for OOC chat. Split into its own partial file,
// matching this repo's convention of splitting manager classes by feature (see NullLinkPlayerManager.*.cs).
internal sealed partial class ChatManager
{
    [Dependency] private ISunsetSponsorTierReader _sunsetSponsorTiers = default!;

    private static readonly Dictionary<int, string> SunsetTierBracketLoc = new()
    {
        { 1, "sunset-sponsor-tier-bracket-zombie" },
        { 2, "sunset-sponsor-tier-bracket-syndicate" },
        { 3, "sunset-sponsor-tier-bracket-vampire" },
        { 4, "sunset-sponsor-tier-bracket-sunsetter" },
        { 5, "sunset-sponsor-tier-bracket-ghost" },
    };

    private bool TryBuildSunsetSponsorWrap(ICommonSession player, string message, [NotNullWhen(true)] out string? wrapped)
    {
        wrapped = null;

        var tier = _sunsetSponsorTiers.GetSponsorTier(player);
        if (tier is < 1 or > 5)
            return false;

        var bracket = Loc.GetString(SunsetTierBracketLoc[tier]);
        var glowingTag = SunsetGlowText(tier, $"{bracket} {player.Name}");

        wrapped = Loc.GetString("chat-manager-send-ooc-sunset-wrap-message",
            ("glowingTag", glowingTag),
            ("message", FormattedMessage.EscapeText(message)));
        return true;
    }

    private bool TryBuildSunsetAdminWrap(ICommonSession player, string message, Color nameColor, Color messageColor, [NotNullWhen(true)] out string? wrapped)
    {
        wrapped = null;

        // Deliberately NOT the NullLink-sourced playerTitle used just above in SendOOC - this reads the
        // fully local Admin/AdminRank DB tables via AdminManager, independent of any external system.
        var title = _adminManager.GetAdminData(player)?.Title;
        if (string.IsNullOrEmpty(title))
            return false;

        wrapped = Loc.GetString("chat-manager-send-ooc-sunset-admin-wrap-message",
            ("adminTitle", title),
            ("nameColor", nameColor),
            ("messageColor", messageColor),
            ("playerName", player.Name),
            ("message", FormattedMessage.EscapeText(message)));
        return true;
    }

    // Wraps every glyph in its own [sunsetglow t=<tier> i=<index>] node instead of a static [color=...].
    // SunsetSponsorGlowTag (client) picks a fresh color per glyph every frame from wall-clock time + index,
    // which is what turns this into an actual shifting shimmer instead of a color baked in at send-time.
    // This also sidesteps needing to hand-escape literal '[' ']' in the bracket loc strings - EscapeText
    // runs on every single glyph (including the bracket's own brackets) before it goes anywhere near markup.
    private static string SunsetGlowText(int tier, string text)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < text.Length; i++)
        {
            sb.Append($"[sunsetglow t={tier} i={i}]{FormattedMessage.EscapeText(text[i].ToString())}[/sunsetglow]");
        }

        return sb.ToString();
    }
}
