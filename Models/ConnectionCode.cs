using Newtonsoft.Json.Linq;

namespace BanGuard.Models;

public class ConnectionCode
{
    public int Code { get; set; }
    public int DurationInSeconds { get; set; }
    public int DurationInMinutes => DurationInSeconds / 60;

    public ConnectionCode(int code, int durationInSeconds)
    {
        Code = code;
        DurationInSeconds = durationInSeconds;
    }

    public static ConnectionCode FromJson(JObject json)
    {
        return new ConnectionCode(
            json["code"]!.ToObject<int>(),
            json["expiration-time-seconds"]!.ToObject<int>()
        );
    }
}