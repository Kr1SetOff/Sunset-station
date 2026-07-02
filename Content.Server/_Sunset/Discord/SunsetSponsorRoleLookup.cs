using System.Threading.Tasks;
using Content.Server.Discord.DiscordLink;
using Content.Shared._Sunset.CCVar;
using Robust.Shared.Configuration;

namespace Content.Server._Sunset.Discord;

/// <summary>
/// Maps a linked Discord account's guild roles to a Boosty sponsor tier (0-5), using the 5 configured
/// tier role ids and the existing Discord bot connection to read guild membership.
/// </summary>
public sealed partial class SunsetSponsorRoleLookup : IPostInjectInit
{
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private DiscordLink _discordLink = default!;
    [Dependency] private ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;

    /// <summary>
    /// Returns 0 (no tier) if the guild id isn't configured, the bot can't see the member, or they hold no tier role.
    /// Highest tier wins if a member somehow holds multiple tier roles.
    /// </summary>
    public async Task<int> ResolveTierAsync(ulong discordUserId)
    {
        if (!ulong.TryParse(_cfg.GetCVar(SunsetCCVars.BoostyDiscordGuildId), out var guildId) || guildId == 0)
        {
            _sawmill.Warning("sunset.boosty_discord.guild_id is not configured; cannot resolve sponsor tiers.");
            return 0;
        }

        var roleIds = await _discordLink.GetGuildMemberRoleIdsAsync(guildId, discordUserId);
        if (roleIds == null)
            return 0;

        var roleSet = new HashSet<ulong>(roleIds);

        for (var tier = 5; tier >= 1; tier--)
        {
            if (TryGetTierRoleId(tier, out var tierRoleId) && roleSet.Contains(tierRoleId))
                return tier;
        }

        return 0;
    }

    private bool TryGetTierRoleId(int tier, out ulong roleId)
    {
        CVarDef<string>? cvar = tier switch
        {
            1 => SunsetCCVars.BoostyTier1RoleId,
            2 => SunsetCCVars.BoostyTier2RoleId,
            3 => SunsetCCVars.BoostyTier3RoleId,
            4 => SunsetCCVars.BoostyTier4RoleId,
            5 => SunsetCCVars.BoostyTier5RoleId,
            _ => null,
        };

        roleId = 0;
        return cvar != null && ulong.TryParse(_cfg.GetCVar(cvar), out roleId) && roleId != 0;
    }

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("sunset.sponsor_role_lookup");
    }
}
