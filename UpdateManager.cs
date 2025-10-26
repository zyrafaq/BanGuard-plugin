using Newtonsoft.Json;
using TerrariaApi.Server;
using TShockAPI;

namespace BanGuard;

public static class UpdateManager
{
    public static async Task<Version?> RequestLatestVersion()
    {
        string url = "https://api.github.com/repos/zyrafaq/BanGuard-plugin/releases/latest";

        using (HttpClient client = new HttpClient())
        {
            client.DefaultRequestHeaders.UserAgent.TryParseAdd("request"); // Set a user agent header

            try
            {
                var response = await client.GetStringAsync(url);
                dynamic? latestRelease = JsonConvert.DeserializeObject<dynamic>(response);

                if (latestRelease == null) return null;

                string tag = latestRelease.tag_name;

                tag = tag.Trim('v');
                string[] nums = tag.Split('.');

                Version version = new Version(int.Parse(nums[0]),
                                              int.Parse(nums[1]),
                                              int.Parse(nums[2])
                                              );
                return version;
            }
            catch
            {
                TShock.Log.ConsoleError("[BanGuard] An error occured while checking for updates.");
            }
        }

        return null;
    }

    public static async Task<bool> IsUpToDate(TerrariaPlugin plugin)
    {
        Version? latestVersion = await RequestLatestVersion();
        Version curVersion = plugin.Version;

        return latestVersion != null && curVersion >= latestVersion;
    }

    public static async void CheckUpdateVerbose(TerrariaPlugin? plugin)
    {
        if (plugin == null) return;

        TShock.Log.ConsoleInfo("[BanGuard] Checking for updates...");

        bool isUpToDate = await IsUpToDate(plugin);

        if (isUpToDate)
        {
            TShock.Log.ConsoleInfo("[BanGuard] The plugin is up to date!");
        }
        else
        {
            TShock.Log.ConsoleError("[BanGuard] The plugin is not up to date.\n" +
                                    "Please visit https://github.com/zyrafaq/BanGuard-plugin/releases/latest to download the latest version.");
        }
    }
}