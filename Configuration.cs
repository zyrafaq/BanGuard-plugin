using Newtonsoft.Json;
using TShockAPI;
using System.Collections.Generic;

namespace BanGuard;

public class Configuration
{
    public static readonly string ConfigPath = Path.Combine(TShock.SavePath, "BanGuardConfig.json");
    public string APIKey = "paste_your_api_key_here";
    public bool EnableDiscordConnection = false;
    public bool DisallowProxyIPs = true;
    public List<string> BadBanCategories = new List<string>();
    // You can find the valid categories list at the discord support server
    public string[] ConnectedPlayerPermissions = { };

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