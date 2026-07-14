using System.Diagnostics.CodeAnalysis;
using System.Text;
using Content.Shared._Sunset.CCVar;
using Content.Shared._Sunset.SponsorTier;
using Robust.Shared.Configuration;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.Chat.Managers;

// 🌇Sunset🌇 - Boosty sponsor tier + admin bracket tags for OOC chat. Split into its own partial file,
// matching this repo's convention of splitting manager classes by feature (see NullLinkPlayerManager.*.cs).
internal sealed partial class ChatManager
{
    [Dependency] private ISunsetSponsorTierReader _sunsetSponsorTiers = default!;
    [Dependency] private IGameTiming _sunsetTiming = default!;

    private static readonly Dictionary<int, string> SunsetTierBracketLoc = new()
    {
        { 1, "sunset-sponsor-tier-bracket-zombie" },
        { 2, "sunset-sponsor-tier-bracket-syndicate" },
        { 3, "sunset-sponsor-tier-bracket-vampire" },
        { 4, "sunset-sponsor-tier-bracket-sunsetter" },
        { 5, "sunset-sponsor-tier-bracket-ghost" },
    };

    // Rainbow order walked (and, when animated, phase-shifted) by SunsetRainbowColor.
    private static readonly string[] SunsetRainbowHex =
    {
        "#FF0000", "#FF7F00", "#FFFF00", "#00FF00", "#0000FF", "#4B0082", "#9400D3",
    };

    // How long (in seconds) one full animation loop takes for the alternating tag colors
    // (tier 3 Vampire, tier 5 Ghost) and for one full step of the tier 4 rainbow shift.
    private const float SunsetTagAnimationCycleSeconds = 4f;
    private const float SunsetRainbowShiftSecondsPerStep = 0.5f;

    private bool TryBuildSunsetSponsorWrap(ICommonSession player, string message, [NotNullWhen(true)] out string? wrapped)
    {
        wrapped = null;

        var tier = _sunsetSponsorTiers.GetSponsorTier(player);
        if (tier is < 1 or > 5)
            return false;

        var bracket = Loc.GetString(SunsetTierBracketLoc[tier]);
        var coloredBracket = SunsetColorizeName(tier, bracket);
        var coloredName = SunsetColorizeName(tier, player.Name);

        wrapped = Loc.GetString("chat-manager-send-ooc-sunset-wrap-message",
            ("bracket", coloredBracket),
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

    private string SunsetColorizeName(int tier, string name) => tier switch
    {
        1 => $"[color=#2e8b57]{FormattedMessage.EscapeText(name)}[/color]", // Zombie: swamp/marsh green
        2 => $"[color=#dc143c]{FormattedMessage.EscapeText(name)}[/color]", // Syndicate Agent: scarlet red
        3 => SunsetAlternatingColor(name, Color.FromHex("#660000"), Color.FromHex("#000000")), // Vampire: dark blood red/black alternating per letter
        4 => SunsetRainbowColor(name), // SunSetter: shifting rainbow
        5 => SunsetAlternatingColor(name, Color.FromHex("#FFFFFF"), Color.FromHex("#808080")), // Ghost: white/gray alternating per letter
        _ => FormattedMessage.EscapeText(name),
    };

    /// <summary>
    /// Colors <paramref name="name"/> letter-by-letter, alternating between two colors. With
    /// sunset.chat.sponsor_tag_animated off (or every message, if you like a snapshot), letter 0 is
    /// colorA, letter 1 is colorB, etc. - a fixed pattern. Animated, each letter's blend between the
    /// two colors follows a sine wave, with neighboring letters exactly half a cycle out of phase -
    /// so the whole name continuously breathes from (A,B,A,B..) through a blended midpoint to the
    /// fully inverted (B,A,B,A..) and back, rather than static server-baked-once text. Since chat
    /// lines are colored once at send time and never repainted client-side, "animated" here means
    /// consecutive messages catch different points of the cycle - not that a single already-sent
    /// line visibly moves.
    /// </summary>
    private string SunsetAlternatingColor(string name, Color colorA, Color colorB)
    {
        var animated = _configurationManager.GetCVar(SunsetCCVars.SponsorTagAnimated);
        var sb = new StringBuilder();

        for (var i = 0; i < name.Length; i++)
        {
            Color color;
            if (!animated)
            {
                color = i % 2 == 0 ? colorA : colorB;
            }
            else
            {
                var cyclePhase = (float) (_sunsetTiming.CurTime.TotalSeconds / SunsetTagAnimationCycleSeconds);
                var letterPhase = (cyclePhase + (i % 2) * 0.5f) * MathF.Tau;
                var blend = (MathF.Sin(letterPhase) + 1f) / 2f;
                color = Color.InterpolateBetween(colorA, colorB, blend);
            }

            sb.Append($"[color={color.ToHexNoAlpha()}]{FormattedMessage.EscapeText(name[i].ToString())}[/color]");
        }

        return sb.ToString();
    }

    private string SunsetRainbowColor(string name)
    {
        var animated = _configurationManager.GetCVar(SunsetCCVars.SponsorTagAnimated);
        var shift = 0;
        if (animated)
            shift = (int) (_sunsetTiming.CurTime.TotalSeconds / SunsetRainbowShiftSecondsPerStep) % SunsetRainbowHex.Length;

        var sb = new StringBuilder();
        for (var i = 0; i < name.Length; i++)
        {
            var color = SunsetRainbowHex[(i + shift) % SunsetRainbowHex.Length];
            sb.Append($"[color={color}]{FormattedMessage.EscapeText(name[i].ToString())}[/color]");
        }

        return sb.ToString();
    }
}
