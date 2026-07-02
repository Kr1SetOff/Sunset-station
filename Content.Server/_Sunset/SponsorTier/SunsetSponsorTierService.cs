using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Content.Server._Sunset.Discord;
using Content.Server.Database;
using Content.Shared._Sunset.CCVar;
using Content.Shared._Sunset.SponsorTier;
using Robust.Server.Player;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._Sunset.SponsorTier;

/// <summary>
/// Resolves and caches each connected player's Boosty sponsor tier (0 = none, 1-5 = tier).
/// The tier is looked up once at connect (from the DB cache column) and refreshed on
/// link/manual-refresh/periodic recheck against Discord - it is never resolved live on every read,
/// since it's consumed synchronously from places like ghost theme requirement checks.
/// </summary>
public sealed partial class SunsetSponsorTierService : IPostInjectInit, ISunsetSponsorTierReader
{
    [Dependency] private IServerDbManager _db = default!;
    [Dependency] private UserDbDataManager _userDb = default!;
    [Dependency] private IPlayerManager _player = default!;
    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private SunsetSponsorRoleLookup _roleLookup = default!;
    [Dependency] private ILogManager _logManager = default!;

    private readonly Dictionary<NetUserId, int> _tiers = new();
    private readonly Dictionary<NetUserId, bool> _linked = new();
    private ISawmill _sawmill = default!;

    /// <summary>
    /// Dev/testing override - this ckey always resolves to the max sponsor tier, regardless of any
    /// actual Discord link, so ghost themes and other sponsor-gated content can be tested without a
    /// live Boosty subscription.
    /// </summary>
    private const string DevOverrideCkey = "ClubYT";
    private const int DevOverrideTier = 5;

    private async Task LoadData(ICommonSession session, CancellationToken cancel)
    {
        var link = await _db.GetSunsetDiscordLink(session.UserId, cancel);
        cancel.ThrowIfCancellationRequested();
        _tiers[session.UserId] = link?.SponsorTier ?? 0;
        _linked[session.UserId] = link != null;
    }

    private void ClientDisconnected(ICommonSession session)
    {
        _tiers.Remove(session.UserId);
        _linked.Remove(session.UserId);
    }

    public int GetSponsorTier(NetUserId userId)
    {
        if (_player.TryGetSessionById(userId, out var session) &&
            session.Name.Equals(DevOverrideCkey, StringComparison.OrdinalIgnoreCase))
            return DevOverrideTier;

        return _tiers.GetValueOrDefault(userId, 0);
    }

    public int GetSponsorTier(ICommonSession session) => GetSponsorTier(session.UserId);

    public bool TryGetSponsorTier(ICommonSession session, out int tier)
    {
        tier = GetSponsorTier(session);
        return tier > 0;
    }

    /// <summary>
    /// Whether this (online) player has a linked Discord account at all, regardless of whether it
    /// resolved to an active sponsor tier. Only valid for connected sessions - see <see cref="GetSponsorTierAsync"/>
    /// for an offline-safe equivalent if ever needed.
    /// </summary>
    public bool IsLinked(ICommonSession session) => _linked.GetValueOrDefault(session.UserId, false);

    public async Task<int> GetSponsorTierAsync(NetUserId userId)
    {
        if (_player.TryGetSessionById(userId, out var session) &&
            session.Name.Equals(DevOverrideCkey, StringComparison.OrdinalIgnoreCase))
            return DevOverrideTier;

        if (_tiers.TryGetValue(userId, out var cached))
            return cached;

        var link = await _db.GetSunsetDiscordLink(userId.UserId);
        return link?.SponsorTier ?? 0;
    }

    /// <summary>
    /// Links a player's account to a Discord user id, resolves their current tier from guild roles,
    /// persists it, and updates the in-memory cache immediately if they're online.
    /// </summary>
    public async Task<int> LinkAsync(NetUserId player, ulong discordUserId)
    {
        var tier = await _roleLookup.ResolveTierAsync(discordUserId);
        await _db.SetSunsetDiscordLink(player.UserId, discordUserId.ToString(), tier);

        if (_player.TryGetSessionById(player, out _))
        {
            _tiers[player] = tier;
            _linked[player] = true;
        }

        _sawmill.Info($"Linked player {player} to Discord user {discordUserId}, resolved tier {tier}.");
        return tier;
    }

    /// <summary>
    /// Re-checks a linked player's current Discord roles and refreshes their cached tier (used by the
    /// "refresh status" button and the periodic recheck below). Returns null if the player isn't linked.
    /// </summary>
    public async Task<int?> RefreshAsync(NetUserId player)
    {
        var link = await _db.GetSunsetDiscordLink(player.UserId);
        if (link == null || !ulong.TryParse(link.DiscordUserId, out var discordUserId))
            return null;

        var tier = await _roleLookup.ResolveTierAsync(discordUserId);
        await _db.UpdateSunsetSponsorTier(player.UserId, tier);

        if (_player.TryGetSessionById(player, out _))
            _tiers[player] = tier;

        return tier;
    }

    private async void RecheckOnlinePlayers()
    {
        foreach (var userId in _tiers.Keys.ToArray())
        {
            try
            {
                await RefreshAsync(userId);
            }
            catch (Exception e)
            {
                _sawmill.Error($"Failed periodic sponsor tier recheck for {userId}: {e}");
            }
        }
    }

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("sunset.sponsor_tier");
        _userDb.AddOnLoadPlayer(LoadData);
        _userDb.AddOnPlayerDisconnect(ClientDisconnected);

        var intervalSeconds = _cfg.GetCVar(SunsetCCVars.SunsetSponsorTierRecheckIntervalSeconds);
        if (intervalSeconds > 0)
        {
            Robust.Shared.Timing.Timer.SpawnRepeating(TimeSpan.FromSeconds(intervalSeconds), RecheckOnlinePlayers, CancellationToken.None);
        }
    }
}
