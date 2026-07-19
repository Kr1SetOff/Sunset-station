using System.Threading.Tasks;
using Content.Server.Chat.Managers;
using Content.Shared.CCVar;
using Content.Shared.Chat;
using NetCord;
using NetCord.Gateway;
using Robust.Shared.Asynchronous;
using Robust.Shared.Configuration;

namespace Content.Server.Discord.DiscordLink;

public sealed partial class DiscordChatLink : IPostInjectInit
{
    [Dependency] private DiscordLink _discordLink = default!;
    [Dependency] private DiscordWebhook _discordWebhook = default!;
    [Dependency] private IConfigurationManager _configurationManager = default!;
    [Dependency] private IChatManager _chatManager = default!;
    [Dependency] private ITaskManager _taskManager = default!;
    [Dependency] private ILogManager _logManager = default!;

    private ISawmill _sawmill = default!;

    private ulong? _oocChannelId;
    private ulong? _adminChannelId;
    private WebhookIdentifier? _adminChatWebhookId;

    public void Initialize()
    {
        _discordLink.OnMessageReceived += OnMessageReceived;

        #if DEBUG
        _discordLink.RegisterCommandCallback(OnDebugCommandRun, "debug");
        #endif

        _configurationManager.OnValueChanged(CCVars.OocDiscordChannelId, OnOocChannelIdChanged, true);
        _configurationManager.OnValueChanged(CCVars.AdminChatDiscordChannelId, OnAdminChannelIdChanged, true);
        _configurationManager.OnValueChanged(CCVars.AdminChatDiscordWebhook, OnAdminChatWebhookChanged, true);
    }

    public void Shutdown()
    {
        _discordLink.OnMessageReceived -= OnMessageReceived;

        _configurationManager.UnsubValueChanged(CCVars.OocDiscordChannelId, OnOocChannelIdChanged);
        _configurationManager.UnsubValueChanged(CCVars.AdminChatDiscordChannelId, OnAdminChannelIdChanged);
        _configurationManager.UnsubValueChanged(CCVars.AdminChatDiscordWebhook, OnAdminChatWebhookChanged);
    }

    #if DEBUG
    private void OnDebugCommandRun(CommandReceivedEventArgs ev)
    {
        var args = string.Join('\n', ev.Arguments);
        _sawmill.Info($"Provided arguments: \n{args}");
    }
    #endif

    private void OnOocChannelIdChanged(string channelId)
    {
        if (string.IsNullOrEmpty(channelId))
        {
            _oocChannelId = null;
            return;
        }

        _oocChannelId = ulong.Parse(channelId);
    }

    private void OnAdminChannelIdChanged(string channelId)
    {
        if (string.IsNullOrEmpty(channelId))
        {
            _adminChannelId = null;
            return;
        }

        _adminChannelId = ulong.Parse(channelId);
    }

    private void OnAdminChatWebhookChanged(string url)
    {
        _adminChatWebhookId = null;

        if (string.IsNullOrWhiteSpace(url))
            return;

        _discordWebhook.GetWebhook(url, data => _adminChatWebhookId = data.ToIdentifier());
    }

    private void OnMessageReceived(Message message)
    {
        if (message.Author.IsBot)
            return;

        var contents = message.Content.ReplaceLineEndings(" ");

        if (message.ChannelId == _oocChannelId)
        {
            _taskManager.RunOnMainThread(() => _chatManager.SendHookOOC(message.Author.Username, contents));
        }
        else if (message.ChannelId == _adminChannelId)
        {
            _taskManager.RunOnMainThread(() => _chatManager.SendHookAdmin(message.Author.Username, contents));
        }
    }

    public async void SendMessage(string message, string author, ChatChannel channel)
    {
        // @ and < are both problematic for discord due to pinging. / is sanitized solely to kneecap links to murder embeds via blunt force
        message = message.Replace("@", "\\@").Replace("<", "\\<").Replace("/", "\\/");

        if (channel == ChatChannel.AdminChat)
            await SendAdminChatWebhookMessage(message, author);

        var channelId = channel switch
        {
            ChatChannel.OOC => _oocChannelId,
            ChatChannel.AdminChat => _adminChannelId,
            _ => throw new InvalidOperationException("Channel not linked to Discord."),
        };

        if (channelId == null)
        {
            // Bot channel relay not set up. The webhook relay above (if configured) has already run. Ignore.
            return;
        }

        try
        {
            await _discordLink.SendMessageAsync(channelId.Value, $"**{channel.GetString()}**: `{author}`: {message}");
        }
        catch (Exception e)
        {
            _sawmill.Error($"Error while sending Discord message: {e}");
        }
    }

    // Sunset: separate one-way webhook relay for admin chat, kept independent from the bot-based
    // channel relay above so admin chat and ahelp can be routed to different Discord channels without
    // needing the full bot (token/guild id) configured.
    private async Task SendAdminChatWebhookMessage(string message, string author)
    {
        if (_adminChatWebhookId is not { } identifier)
            return;

        var payload = new WebhookPayload { Content = $"**{author}**: {message}" };

        try
        {
            await _discordWebhook.CreateMessage(identifier, payload);
        }
        catch (Exception e)
        {
            _sawmill.Error($"Error while sending admin chat webhook message: {e}");
        }
    }

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("discord.chat");
    }
}
