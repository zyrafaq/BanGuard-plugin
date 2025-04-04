using Newtonsoft.Json;
using TShockAPI;

namespace BanGuard;

public class Configuration
{
    public static readonly string ConfigPath = Path.Combine(TShock.SavePath, "BanGuardConfig.json");
    public string APIKey = "paste_your_api_key_here";
    public bool CheckPlayerBanOnJoin = true;
    public string ServerJoinMessage = "This server is powered by BanGuard.";

    public void Write()
    {
        File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(this, Formatting.Indented));
    }

    public static Configuration Reload()
    {
        Configuration? c = null;

        if (File.Exists(ConfigPath)) c = JsonConvert.DeserializeObject<Configuration>(File.ReadAllText(ConfigPath));

        c ??= new Configuration();

        File.WriteAllText(ConfigPath, JsonConvert.SerializeObject(c, Formatting.Indented));

        return c;
    }
}