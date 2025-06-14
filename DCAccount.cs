using Newtonsoft.Json.Linq;

namespace BanGuard;

public class DCAccount
{

    public string DiscordName { get; set; }
    public string DiscordID { get; set; }

    public DCAccount(string discordName, string discordID)
    {
        DiscordName = discordName;
        DiscordID = discordID;
    }

    internal static DCAccount? FromJson(JObject jObject)
    {
        try
        {
            string discordName = jObject["Discord username"]!.ToString();
            string discordID = jObject["Discord ID"]!.ToString();
            bool connected = jObject["connected"]!.Value<bool>();

            if (string.IsNullOrWhiteSpace(discordName) || string.IsNullOrWhiteSpace(discordID) || !connected)
            {
                return null;
            }

            return new DCAccount(discordName, discordID);
        }
        catch
        {
            return null;
        }
    }
}