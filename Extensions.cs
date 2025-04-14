using TShockAPI;

namespace BanGuard;

public static class Extensions
{
    public static DCAccount? GetDiscordAccount(this TSPlayer player)
    {
        return player.GetData<DCAccount?>("BanGuard.DiscordAccount");
    }

    public static void SetDiscordAccount(this TSPlayer player, DCAccount account)
    {
        player.SetData("BanGuard.DiscordAccount", account);
    }
}