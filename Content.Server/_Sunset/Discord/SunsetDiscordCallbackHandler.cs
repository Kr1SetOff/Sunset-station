using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Content.Server._Sunset.SponsorTier;
using Robust.Server.ServerStatus;
using Robust.Shared.Asynchronous;

namespace Content.Server._Sunset.Discord;

/// <summary>
/// Receives the Discord OAuth2 redirect (the player's browser lands here after authorizing).
/// Registered directly on the existing status-host HTTP listener - no separate web server needed.
/// Not routed through <see cref="Content.Server.Administration.ServerApi"/> since that requires an
/// admin SS14Token, which a plain browser redirect can't supply.
/// </summary>
public sealed partial class SunsetDiscordCallbackHandler : IPostInjectInit
{
    private const string CallbackPath = "/sunset/discord/callback";

    [Dependency] private IStatusHost _statusHost = default!;
    [Dependency] private SunsetDiscordOAuth _oauth = default!;
    [Dependency] private SunsetSponsorTierService _tierService = default!;
    [Dependency] private ITaskManager _taskManager = default!;
    [Dependency] private ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("sunset.discord_callback");
        _statusHost.AddHandler(HandleCallback);
    }

    private async Task<bool> HandleCallback(IStatusHandlerContext context)
    {
        if (context.RequestMethod != HttpMethod.Get || context.Url.AbsolutePath != CallbackPath)
            return false;

        var query = ParseQuery(context.Url.Query);
        query.TryGetValue("code", out var code);
        query.TryGetValue("state", out var state);

        if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
        {
            await Respond(context, HttpStatusCode.BadRequest, "Missing code/state.");
            return true;
        }

        if (!_oauth.TryValidateState(state, out var player))
        {
            _sawmill.Warning("Rejected Discord callback with invalid/expired state.");
            await Respond(context, HttpStatusCode.BadRequest, "Invalid or expired link request. Please try again from the game.");
            return true;
        }

        var discordUserId = await _oauth.ExchangeCodeForDiscordUserIdAsync(code);
        if (discordUserId is not { } id)
        {
            await Respond(context, HttpStatusCode.BadGateway, "Could not verify your Discord account. Please try again.");
            return true;
        }

        var tier = await RunOnMainThread(() => _tierService.LinkAsync(player, id));

        await Respond(context, HttpStatusCode.OK, tier > 0
            ? "Discord linked! Your sponsor tier was applied. You can close this tab."
            : "Discord linked! No active sponsor role was found on our Discord server. You can close this tab.");
        return true;
    }

    private static Dictionary<string, string> ParseQuery(string query)
    {
        var result = new Dictionary<string, string>();
        if (string.IsNullOrEmpty(query))
            return result;

        var trimmed = query.StartsWith('?') ? query[1..] : query;
        foreach (var pair in trimmed.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var idx = pair.IndexOf('=');
            if (idx < 0)
                continue;

            var key = Uri.UnescapeDataString(pair[..idx]);
            var value = Uri.UnescapeDataString(pair[(idx + 1)..]);
            result[key] = value;
        }

        return result;
    }

    private static async Task Respond(IStatusHandlerContext context, HttpStatusCode code, string message)
    {
        var html = $"<html><body style=\"font-family:sans-serif;text-align:center;margin-top:10%\"><h2>{WebUtility.HtmlEncode(message)}</h2></body></html>";
        await context.RespondAsync(html, code, "text/html");
    }

    /// <summary>
    /// Marshals DB/session-touching work onto the main thread, matching the pattern in
    /// <c>Content.Server/Administration/ServerApi.Utility.cs</c> - the status host invokes handlers off-thread.
    /// </summary>
    private Task<T> RunOnMainThread<T>(Func<Task<T>> func)
    {
        var tcs = new TaskCompletionSource<T>();
        _taskManager.RunOnMainThread(async () =>
        {
            try
            {
                tcs.TrySetResult(await func());
            }
            catch (Exception e)
            {
                tcs.TrySetException(e);
            }
        });

        return tcs.Task;
    }
}
