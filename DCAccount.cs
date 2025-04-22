using Newtonsoft.Json.Linq;
using TShockAPI;

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

            return new DCAccount(discordName, discordID);
        }
        catch
        {
            return null;
        }
    }
}