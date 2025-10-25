namespace BanGuard.Models;

public class PlayerBan
{
    public bool IsBanned { get; set; }
    public bool IsProxy { get; set; }
    public List<Dictionary<dynamic, dynamic>> Bans { get; set; }

    public PlayerBan(bool isBanned, bool isProxy, List<Dictionary<dynamic, dynamic>> bans)
    {
        IsBanned = isBanned;
        IsProxy = isProxy;
        Bans = bans;
    }
}
