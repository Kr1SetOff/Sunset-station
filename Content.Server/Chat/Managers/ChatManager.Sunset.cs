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

    // Static per-letter rainbow (no animation) - see SunsetRainbowColor.
    private static readonly string[] SunsetRainbowHex =
    {
        "#FF0000", "#FF7F00", "#FFFF00", "#00FF00", "#0000FF", "#4B0082", "#9400D3",
    };

    private bool TryBuildSunsetSponsorWrap(ICommonSession player, string message, [NotNullWhen(true)] out string? wrapped)
    {
        wrapped = null;

        var tier = _sunsetSponsorTiers.GetSponsorTier(player);
        if (tier is < 1 or > 5)
            return false;

        var bracket = Loc.GetString(SunsetTierBracketLoc[tier]);
        var coloredName = SunsetColorizeName(tier, player.Name);

        wrapped = Loc.GetString("chat-manager-send-ooc-sunset-wrap-message",
            ("bracket", bracket),
            ("coloredName", coloredName),
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

    private static string SunsetColorizeName(int tier, string name) => tier switch
    {
        1 => $"[color=#2e8b57]{FormattedMessage.EscapeText(name)}[/color]", // Zombie: swamp/marsh green
        2 => $"[color=#dc143c]{FormattedMessage.EscapeText(name)}[/color]", // Syndicate Agent: scarlet red
        3 => SunsetAlternatingColor(name, "#dc143c", "#000000"), // Vampire: red/black alternating per letter
        4 => SunsetRainbowColor(name), // SunSetter: static per-letter rainbow
        5 => $"[color=#FFFFFFAA]{FormattedMessage.EscapeText(name)}[/color]", // Ghost: translucent white
        _ => FormattedMessage.EscapeText(name),
    };

    private static string SunsetAlternatingColor(string name, string colorA, string colorB)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var color = i % 2 == 0 ? colorA : colorB;
            sb.Append($"[color={color}]{FormattedMessage.EscapeText(name[i].ToString())}[/color]");
        }

        return sb.ToString();
    }

    private static string SunsetRainbowColor(string name)
    {
        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var color = SunsetRainbowHex[i % SunsetRainbowHex.Length];
            sb.Append($"[color={color}]{FormattedMessage.EscapeText(name[i].ToString())}[/color]");
        }

        return sb.ToString();
    }
}
