using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Content.Shared._Sunset.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Network;

namespace Content.Server._Sunset.Discord;

/// <summary>
/// Builds/validates the Discord OAuth2 "link your account" flow and exchanges the resulting
/// authorization code for the player's Discord user id. Fully self-contained - does not depend
/// on the external _NullLink cluster.
/// </summary>
public sealed partial class SunsetDiscordOAuth : IPostInjectInit
{
    private const string Scope = "identify guilds.members.read";

    [Dependency] private IConfigurationManager _cfg = default!;
    [Dependency] private ILogManager _logManager = default!;

    private readonly HttpClient _http = new();
    private ISawmill _sawmill = default!;

    /// <summary>
    /// Builds the "authorize" URL to send the player's browser to. Returns an empty string if OAuth isn't configured yet.
    /// </summary>
    public string GetAuthUrl(NetUserId player)
    {
        var clientId = _cfg.GetCVar(SunsetCCVars.BoostyDiscordClientId);
        var redirectUri = _cfg.GetCVar(SunsetCCVars.BoostyDiscordRedirectUri);
        var secret = _cfg.GetCVar(SunsetCCVars.BoostyDiscordStateSecret);

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(redirectUri) || string.IsNullOrEmpty(secret))
        {
            _sawmill.Warning("Sunset Discord OAuth is not configured (client_id/redirect_uri/state_secret); link button will do nothing.");
            return string.Empty;
        }

        var state = SignState(player.UserId.ToString(), secret);
        return "https://discord.com/api/oauth2/authorize" +
               $"?client_id={Uri.EscapeDataString(clientId)}" +
               $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
               "&response_type=code" +
               $"&scope={Uri.EscapeDataString(Scope)}" +
               $"&state={Uri.EscapeDataString(state)}";
    }

    /// <summary>
    /// Verifies the HMAC-signed "state" query parameter Discord echoes back to the callback,
    /// recovering which local player initiated the flow without needing a server-side session cache.
    /// </summary>
    public bool TryValidateState(string state, out NetUserId userId)
    {
        userId = default;

        var secret = _cfg.GetCVar(SunsetCCVars.BoostyDiscordStateSecret);
        if (string.IsNullOrEmpty(secret))
            return false;

        var lastDot = state.LastIndexOf('.');
        if (lastDot <= 0 || lastDot == state.Length - 1)
            return false;

        var payload = state[..lastDot];
        var signature = state[(lastDot + 1)..];
        var expected = ComputeSignature(payload, secret);

        if (signature.Length != expected.Length ||
            !CryptographicOperations.FixedTimeEquals(Encoding.UTF8.GetBytes(signature), Encoding.UTF8.GetBytes(expected)))
        {
            return false;
        }

        if (!Guid.TryParse(payload, out var guid))
            return false;

        userId = new NetUserId(guid);
        return true;
    }

    /// <summary>
    /// Exchanges an OAuth2 authorization code for the linked Discord account's user id (snowflake).
    /// Returns null on any failure - callers should treat that as "linking failed, try again".
    /// </summary>
    public async Task<ulong?> ExchangeCodeForDiscordUserIdAsync(string code)
    {
        var clientId = _cfg.GetCVar(SunsetCCVars.BoostyDiscordClientId);
        var clientSecret = _cfg.GetCVar(SunsetCCVars.BoostyDiscordClientSecret);
        var redirectUri = _cfg.GetCVar(SunsetCCVars.BoostyDiscordRedirectUri);

        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret) || string.IsNullOrEmpty(redirectUri))
        {
            _sawmill.Error("Sunset Discord OAuth is not fully configured (missing client_id/client_secret/redirect_uri).");
            return null;
        }

        try
        {
            var form = new Dictionary<string, string>
            {
                ["grant_type"] = "authorization_code",
                ["code"] = code,
                ["redirect_uri"] = redirectUri,
                ["client_id"] = clientId,
                ["client_secret"] = clientSecret,
            };

            using var tokenResponse = await _http.PostAsync(
                "https://discord.com/api/oauth2/token",
                new FormUrlEncodedContent(form));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                _sawmill.Error($"Discord OAuth token exchange failed: {tokenResponse.StatusCode}");
                return null;
            }

            var token = await tokenResponse.Content.ReadFromJsonAsync<DiscordTokenResponse>();
            if (token?.AccessToken is not { } accessToken)
                return null;

            using var userRequest = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/users/@me");
            userRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            using var userResponse = await _http.SendAsync(userRequest);
            if (!userResponse.IsSuccessStatusCode)
            {
                _sawmill.Error($"Discord user info fetch failed: {userResponse.StatusCode}");
                return null;
            }

            var user = await userResponse.Content.ReadFromJsonAsync<DiscordUserResponse>();
            if (user?.Id is not { } id || !ulong.TryParse(id, out var discordId))
                return null;

            return discordId;
        }
        catch (Exception e)
        {
            _sawmill.Error($"Discord OAuth exchange threw: {e}");
            return null;
        }
    }

    void IPostInjectInit.PostInject()
    {
        _sawmill = _logManager.GetSawmill("sunset.discord_oauth");
    }

    private static string SignState(string payload, string secret) => $"{payload}.{ComputeSignature(payload, secret)}";

    private static string ComputeSignature(string payload, string secret)
    {
        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
        var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    private sealed class DiscordTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }
    }

    private sealed class DiscordUserResponse
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }
    }
}
