using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.DB;
using TShockAPI.Hooks;

namespace BanGuard;

public static class Handlers
{
    public static void Initialize()
    {
        ServerApi.Hooks.NetGetData.Register(BanGuard.Instance, OnNetGetData);
        GeneralHooks.ReloadEvent += OnReload;

        if (BanGuard.Config.EnableDiscordConnection)
        {
            ServerApi.Hooks.ServerJoin.Register(BanGuard.Instance, OnServerJoin);
            PlayerHooks.PlayerPermission += OnPlayerPermission;
        }
    }

    public static void Dispose()
    {
        ServerApi.Hooks.NetGetData.Deregister(BanGuard.Instance, OnNetGetData);
        GeneralHooks.ReloadEvent -= OnReload;
        ServerApi.Hooks.ServerJoin.Deregister(BanGuard.Instance, OnServerJoin);
        PlayerHooks.PlayerPermission -= OnPlayerPermission;
    }

    public static void Reload()
    {
        Dispose();
        Initialize();
    }

    private static void OnReload(ReloadEventArgs args)
    {
        BanGuard.Config = Configuration.Reload();
        APIService.Initialize();
        Reload();
        args.Player.SendSuccessMessage("[BanGuard] Plugin has been reloaded.");
    }

    private static async void OnNetGetData(GetDataEventArgs args)
    {
        if (args.MsgID != PacketTypes.ContinueConnecting2) return;

        var player = TShock.Players[args.Msg.whoAmI];
        if (player == null || player.State > 1) return;

        bool isBanned = await APIService.CheckPlayerBan(player.UUID, player.Name, player.IP) ?? false;

        if (isBanned)
        {
            player.Disconnect("You are banned on the BanGuard network.\nVisit https://banguard.uk for more details.");
        }
    }

    private static void OnServerJoin(JoinEventArgs args)
    {
        var player = TShock.Players[args.Who];
        if (player == null) return;

        Task.Run(async () =>
        {
            DCAccount? acc = await APIService.TryGetDiscordAccount(player.UUID);

            if (acc != null) player.SetDiscordAccount(acc);
        });
    }

    private static void OnPlayerPermission(PlayerPermissionEventArgs e)
    {
        if (!e.Player.IsLoggedIn) return;

        if (e.Player.GetDiscordAccount() != null && BanGuard.Config.ConnectedPlayerPermissions.Contains(e.Permission))
        {
            e.Result = PermissionHookResult.Granted;
        }
    }
}