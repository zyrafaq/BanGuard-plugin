using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace BanGuard;

public static class Handlers
{
    public static void Initialize()
    {
        ServerApi.Hooks.NetGetData.Register(BanGuard.Instance, OnNetGetData);
        ServerApi.Hooks.ServerJoin.Register(BanGuard.Instance, OnServerJoin);
        GeneralHooks.ReloadEvent += OnReload;
    }

    public static void Dispose()
    {
        ServerApi.Hooks.NetGetData.Deregister(BanGuard.Instance, OnNetGetData);
        ServerApi.Hooks.ServerJoin.Deregister(BanGuard.Instance, OnServerJoin);
        GeneralHooks.ReloadEvent -= OnReload;
    }

    public static void Reload()
    {
        ServerApi.Hooks.NetGetData.Register(BanGuard.Instance, OnNetGetData);
    }

    private static void OnReload(ReloadEventArgs args)
    {
        BanGuard.Config = Configuration.Reload();

        args.Player.SendSuccessMessage("[BanGuard] Plugin has been reloaded.");
    }

    private static async void OnNetGetData(GetDataEventArgs args)
    {
        if (args.MsgID != PacketTypes.ContinueConnecting2) return;

        var player = TShock.Players[args.Msg.whoAmI];

        if (player == null || player.State > 1) return;

        args.Handled = true;
        int prevState = player.State;
        player.State = 0;

        bool isBanned = await APIService.CheckPlayerBan(player.UUID, player.Name, player.IP) ?? false;

        if (isBanned)
        {
            player.Disconnect("You are banned on the BanGuard network.\nVisit https://banguard.uk for more details.");
        }
        else
        {
            args.Handled = false;
            player.State = prevState + 1;
            player.SendData(PacketTypes.WorldInfo);
        }
    }

    private static void OnServerJoin(JoinEventArgs args)
    {
        var player = TShock.Players[args.Who];
        if (player == null) return;

        player.SendInfoMessage("This server is powered by BanGuard.");
    }
}