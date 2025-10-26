using BanGuard.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TShockAPI;

namespace BanGuard;

public static class APIService
{
    private static readonly HttpClient _client = new HttpClient();
    private static string _apiKey => BanGuard.Config.APIKey;
    private static bool _isApiKeyValid = false;
    private static readonly string _rootURL = "https://banguard.uk/api/";
    private static HttpRequestMessage _checkMessage => new HttpRequestMessage(HttpMethod.Post, _rootURL + "check-player-ban");
    private static HttpRequestMessage _tokenMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "check-token");
    private static HttpRequestMessage _banMessage => new HttpRequestMessage(HttpMethod.Post, _rootURL + "ban-player");
    private static HttpRequestMessage _getPlayerMessage => new HttpRequestMessage(HttpMethod.Post, _rootURL + "get-player");
    private static HttpRequestMessage _getPlayerNamesMessage => new HttpRequestMessage(HttpMethod.Post, _rootURL + "get-player-names");

    private static async Task<JObject?> SendApiRequest(HttpRequestMessage message, Dictionary<string, string>? data = null, bool checkSuccess = true)
    {
        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", _apiKey);

        if (data != null)
        {
            var content = new FormUrlEncodedContent(data);
            message.Content = content;
        }

        HttpResponseMessage response = await _client.SendAsync(message);

        if (checkSuccess) response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        var jsonResponse = JsonConvert.DeserializeObject<JObject>(responseBody);

        return jsonResponse;
    }

    public static void Initialize()
    {
        Task.Run(async () =>
        {
            _isApiKeyValid = await CheckToken();

            if (!_isApiKeyValid)
            {
                TShock.Log.ConsoleError($"Error validating BanGuard API key. Please check your configuration.");
                Environment.Exit(0);
            }
        });
    }

    public static async Task<bool> CheckToken()
    {
        try
        {
            JObject? response = await SendApiRequest(_tokenMessage);
            return response!["valid"]!.ToObject<bool>();
        }
        catch
        {
            return false;
        }
    }

    public static async Task<PlayerBan?> CheckPlayerBan(string uuid, string playerName, string playerIP)
    {
        var requestData = new Dictionary<string, string>
            {
                { "player_uuid", uuid },
                { "player_name", playerName },
                { "player_ip", playerIP },
                { "bad_ban_categories", string.Join(",", BanGuard.Config.BadBanCategories) },
            };
        try
        {
            JObject? response = await SendApiRequest(_checkMessage, requestData);
            bool isBanned = response!["banned"]!.ToObject<bool>();
            bool isProxy;
            if (playerIP != "127.0.0.1")
            {
                isProxy = response!["is_proxy"]!.ToObject<bool>();
            }
            else {
                isProxy = false;
            }
            List<Dictionary<dynamic, dynamic>> bans = response!["bans"]!.ToObject<List<Dictionary<dynamic, dynamic>>>()!;
            List<string> banCategories = response!["ban categories"]!.ToObject<List<string>>()!;
            PlayerBan result = new PlayerBan(isBanned, isProxy, bans, banCategories);
            return result;
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"Error checking player ban: {ex.Message}");
            return null;
        }
    }

    public static async Task<APIResponse<bool>> BanPlayer(string uuid, string category, string ip)
    {
        var requestData = new Dictionary<string, string>
            {
                { "player_uuid", uuid },
                { "player_ip", ip },
                { "category", category }
            };

        try
        {
            JObject? response = await SendApiRequest(_banMessage, requestData, checkSuccess: false);
            if (response!.ContainsKey("valid_categories"))
            {
                return new APIResponse<bool>(false, errorMessage: "Invalid category. Valid categories: " + string.Join(", ", response["valid_categories"]!.ToObject<List<string>>()!));
            }

            return new APIResponse<bool>(true, true);
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"Error banning player: {ex.Message}");
            return new APIResponse<bool>(false, errorMessage: ex.Message);
        }
    }

    public static async Task<List<string>?> GetPlayerNames(string uuid)
    {
        var requestData = new Dictionary<string, string>
            {
                { "player_uuid", uuid }
            };

        try
        {
            JObject? response = await SendApiRequest(_getPlayerNamesMessage, requestData);

            List<string> playerNames = response!["player_names"]!.Select(p => p["name"]!.ToString()).ToList();

            return playerNames;
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"Error getting player names: {ex.Message}");
            return null;
        }
    }
}
