using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TShockAPI;

namespace BanGuard;

public static class APIService
{
    private static readonly HttpClient _client = new HttpClient();
    private static string _apiKey => BanGuard.Config.APIKey;
    private static readonly string _rootURL = "https://banguard.uk/api/";
    private static HttpRequestMessage _generateMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "generate-connection-code");
    private static HttpRequestMessage _newConnectionMessage => new HttpRequestMessage(HttpMethod.Post, _rootURL + "new-connection-code");
    private static HttpRequestMessage _checkMessage => new HttpRequestMessage(HttpMethod.Get, _rootURL + "check-player-ban");


    private static async Task<JObject?> SendApiRequest(HttpRequestMessage message, Dictionary<string, string>? data = null)
    {
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


    public static async Task<bool?> CheckPlayerBan(string uuid, string playerName)
    {
        var requestData = new Dictionary<string, string>
            {
                { "player_uuid", uuid },
                { "player_name", playerName }
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
}
