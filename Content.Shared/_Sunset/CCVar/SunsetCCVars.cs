using Robust.Shared.Configuration;

namespace Content.Shared._Sunset.CCVar;

/// <summary>
/// CVars for the Sunset Discord account linking + Boosty sponsor tier system.
/// Independent of the _NullLink/_Starlight discord.* CVars - this is a self-contained OAuth flow.
/// </summary>
[CVarDefs]
public sealed partial class SunsetCCVars
{
    public static readonly CVarDef<string> BoostyDiscordClientId =
        CVarDef.Create("sunset.boosty_discord.client_id", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> BoostyDiscordClientSecret =
        CVarDef.Create("sunset.boosty_discord.client_secret", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> BoostyDiscordRedirectUri =
        CVarDef.Create("sunset.boosty_discord.redirect_uri", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> BoostyDiscordGuildId =
        CVarDef.Create("sunset.boosty_discord.guild_id", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// Secret used to HMAC-sign the OAuth "state" parameter, so the callback can trust which local player initiated the flow.
    /// </summary>
    public static readonly CVarDef<string> BoostyDiscordStateSecret =
        CVarDef.Create("sunset.boosty_discord.state_secret", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    // One Discord role id per sponsor tier (1-5). Boosty's own Discord integration grants these roles to subscribers.
    public static readonly CVarDef<string> BoostyTier1RoleId =
        CVarDef.Create("sunset.boosty_discord.tier1_role_id", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> BoostyTier2RoleId =
        CVarDef.Create("sunset.boosty_discord.tier2_role_id", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> BoostyTier3RoleId =
        CVarDef.Create("sunset.boosty_discord.tier3_role_id", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> BoostyTier4RoleId =
        CVarDef.Create("sunset.boosty_discord.tier4_role_id", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    public static readonly CVarDef<string> BoostyTier5RoleId =
        CVarDef.Create("sunset.boosty_discord.tier5_role_id", "", CVar.SERVERONLY | CVar.CONFIDENTIAL);

    /// <summary>
    /// How often (in seconds) an online player's sponsor tier is re-checked against Discord, to catch lapsed/renewed Boosty subscriptions.
    /// </summary>
    public static readonly CVarDef<int> SunsetSponsorTierRecheckIntervalSeconds =
        CVarDef.Create("sunset.boosty_discord.tier_recheck_interval_seconds", 3600 * 3, CVar.SERVERONLY);

    /// <summary>
    /// Safety cap on how many tier-5 "guaranteed antagonist" forced assignments can happen in a single round,
    /// since the 99% guarantee bypasses the normal PlayerRatio/Max round-balance limits.
    /// </summary>
    public static readonly CVarDef<int> SunsetTier5MaxForcedAntagsPerRound =
        CVarDef.Create("sunset.boosty_discord.tier5_max_forced_per_round", 5, CVar.SERVER);
}
