using System.Text;
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
    private static HttpRequestMessage _generateMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "generate-connection-code");
    private static HttpRequestMessage _newConnectionMessage => new HttpRequestMessage(HttpMethod.Post, _rootURL + "new-connection-code");
    private static HttpRequestMessage _checkMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "check-player-ban");
    private static HttpRequestMessage _tokenMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "check-token");
    private static HttpRequestMessage _banMessage => new HttpRequestMessage(HttpMethod.Post, _rootURL + "ban-player");


    private static async Task<JObject?> SendApiRequest(HttpRequestMessage message, Dictionary<string, string>? data = null, bool checkToken = true)
    {
        if (checkToken && !_isApiKeyValid)
        {
            TShock.Log.ConsoleError("BanGuard API key is not valid. Please check your configuration.");
            return null;
        }

        _client.DefaultRequestHeaders.Clear();
        _client.DefaultRequestHeaders.Add("Authorization", _apiKey);

        if (data != null)
        {
            var content = new FormUrlEncodedContent(data);
            message.Content = content;
        }

        HttpResponseMessage response = await _client.SendAsync(message);
        response.EnsureSuccessStatusCode();

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
            }
        });
    }

    public static async Task<bool> CheckToken()
    {
        try
        {
            JObject? response = await SendApiRequest(_tokenMessage, checkToken: false);
            return response!["valid"]!.ToObject<bool>();
        }
        catch
        {
            return false;
        }
    }

    public static async Task<bool?> CheckPlayerBan(string uuid, string playerName, string playerIP)
    {
        var requestData = new Dictionary<string, string>
            {
                { "player_uuid", uuid },
                { "player_name", playerName },
                { "player_ip", playerIP}
            };

        try
        {
            JObject? response = await SendApiRequest(_checkMessage, requestData);
            return response!["banned"]!.ToObject<bool>();
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"Error checking player ban: {ex.Message}");
            return null;
        }
    }

    public static async Task<int?> GenerateValidConnectionCode()
    {
        try
        {
            JObject? response = await SendApiRequest(_generateMessage);
            return int.Parse(response!["code"]!.ToString());
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"Error generating connection code: {ex.Message}");
            return null;
        }
    }

    public static async Task<int?> GenerateNewConnection(string uuid, string playerName)
    {
        try
        {
            int? code = await GenerateValidConnectionCode();
            var requestData = new Dictionary<string, string>
                {
                    { "code", code.ToString()!},
                    { "uuid", uuid },
                    { "username", playerName }
                };

            JObject? response = await SendApiRequest(_newConnectionMessage, requestData);
            return code;
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"Error generating connection code: {ex.Message}");
            return null;
        }
    }

    public static async Task<bool> BanPlayer(string uuid, string category, string ip)
    {
        var requestData = new Dictionary<string, string>
            {
                { "player", uuid },
                { "category", category },
                { "player_ip", ip }
            };

        try
        {
            JObject? response = await SendApiRequest(_banMessage, requestData);
            return response != null;
        }
        catch (Exception ex)
        {
            TShock.Log.ConsoleError($"Error banning player: {ex.Message}");
            return false;
        }
    }
}
