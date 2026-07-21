using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Content.Server.Database;
using Content.Server.Discord.DiscordLink;
using Content.Shared.Players.PlayTimeTracking;
using Content.Shared.Roles;
using Robust.Shared.Network;
using Robust.Shared.Prototypes;

namespace Content.Server._Sunset.Discord;

/// <summary>
/// Discord bot command "timers": replies with the invoker's total and per-role playtime on this
/// server, resolved through their Sunset Discord link (see <see cref="SunsetDiscordLinkEui"/>).
/// Players who haven't linked get pointed at the in-game link window instead.
/// </summary>
public sealed class SunsetDiscordTimersCommand : IPostInjectInit
{
    [Dependency] private DiscordLink _discordLink = default!;
    [Dependency] private IServerDbManager _db = default!;
    [Dependency] private IPrototypeManager _prototypes = default!;
    [Dependency] private ILogManager _logManager = default!;

    private const int MaxRoleLines = 25;

    private ISawmill _sawmill = default!;

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("sunset.discord_timers");
        _discordLink.RegisterCommandCallback(OnTimersCommand, "timers");
    }

    private async void OnTimersCommand(CommandReceivedEventArgs ev)
    {
        try
        {
            await HandleTimersAsync(ev);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Failed to handle Discord timers command: {e}");
        }
    }

    private async Task HandleTimersAsync(CommandReceivedEventArgs ev)
    {
        var channelId = ev.Message.ChannelId;
        var discordUserId = ev.Message.Author.Id.ToString();

        var link = await _db.GetSunsetDiscordLinkByDiscordId(discordUserId);
        if (link == null)
        {
            await _discordLink.SendMessageAsync(channelId,
                $"<@{discordUserId}> к этому Discord-аккаунту не привязан игровой аккаунт. Привяжите его через окно привязки в игре.");
            return;
        }

        var record = await _db.GetPlayerRecordByUserId(new NetUserId(link.PlayerUserId));
        var playTimes = await _db.GetPlayTimes(link.PlayerUserId);

        var userName = record?.LastSeenUserName ?? link.PlayerUserId.ToString();

        var overall = playTimes.FirstOrDefault(t => t.Tracker == PlayTimeTrackingShared.TrackerOverall.Id)?.TimeSpent
                      ?? TimeSpan.Zero;

        // tracker id -> localized job name, so "JobSalvageLead" renders as the actual role name.
        var trackerNames = new Dictionary<string, string>();
        foreach (var job in _prototypes.EnumeratePrototypes<JobPrototype>())
            trackerNames.TryAdd(job.PlayTimeTracker, job.LocalizedName);

        var sb = new StringBuilder();
        sb.AppendLine($"**Наигранное время игрока `{Sanitize(userName)}`**");
        sb.AppendLine($"Всего: **{FormatTime(overall)}**");

        var roleTimes = playTimes
            .Where(t => t.Tracker != PlayTimeTrackingShared.TrackerOverall.Id && t.TimeSpent > TimeSpan.Zero)
            .OrderByDescending(t => t.TimeSpent)
            .ToList();

        if (roleTimes.Count > 0)
        {
            sb.AppendLine();
            sb.AppendLine("По ролям:");
            foreach (var time in roleTimes.Take(MaxRoleLines))
            {
                var name = trackerNames.TryGetValue(time.Tracker, out var jobName)
                    ? jobName
                    : time.Tracker;
                sb.AppendLine($"• {Sanitize(name)} — {FormatTime(time.TimeSpent)}");
            }

            if (roleTimes.Count > MaxRoleLines)
                sb.AppendLine($"…и ещё {roleTimes.Count - MaxRoleLines} ролей.");
        }

        await _discordLink.SendMessageAsync(channelId, sb.ToString());
    }

    private static string FormatTime(TimeSpan time)
        => $"{(int) time.TotalHours} ч {time.Minutes} мин";

    private static string Sanitize(string text)
        => text.Replace("@", "\\@").Replace("<", "\\<").Replace("`", "'");
}
