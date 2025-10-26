using BanGuard.Models;
using TerrariaApi.Server;
using TShockAPI;
using TShockAPI.Hooks;

namespace BanGuard;

public static class Handlers
{
    public static void Initialize()
    {
        ServerApi.Hooks.NetGetData.Register(BanGuard.Instance, OnNetGetData);
        ServerApi.Hooks.GamePostInitialize.Register(BanGuard.Instance, OnGamePostInitialize);
        GeneralHooks.ReloadEvent += OnReload;
    }

    public static void Dispose()
    {
        ServerApi.Hooks.NetGetData.Deregister(BanGuard.Instance, OnNetGetData);
        ServerApi.Hooks.GamePostInitialize.Deregister(BanGuard.Instance, OnGamePostInitialize);
        GeneralHooks.ReloadEvent -= OnReload;
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
        Commands.Initialize();
        Reload();
        args.Player.SendSuccessMessage("[BanGuard] Plugin has been reloaded.");
    }

    private static async void OnNetGetData(GetDataEventArgs args)
    {
        if (args.MsgID != PacketTypes.ContinueConnecting2) return;

        var player = TShock.Players[args.Msg.whoAmI];
        if (player == null || player.State > 1) return;


        PlayerBan? playerBan = await APIService.CheckPlayerBan(player.UUID, player.Name, player.IP);

        if (playerBan == null)
        {
            return;
        }

        if (playerBan.IsBanned)
        {
            player.Disconnect(
                $"You are banned on the BanGuard network for {string.Join(", ", playerBan.Categories)}.\n" +
                $"Ban {(playerBan.Bans.Count > 1 ? "IDs" : "ID")}: " +
                $"{string.Join(", ", playerBan.Bans.Select(b => b["id"].ToString()))}"
            );
        }
        else if (playerBan.IsProxy && BanGuard.Config.DisallowProxyIPs)
        {
            player.Disconnect("Proxy or VPN detected. Please disable it to join the server.");
        }
        // They are free to go
    }

    private static void OnGamePostInitialize(EventArgs args)
    {
        if (BanGuard.Config.CheckForUpdates)
        {
            UpdateManager.CheckUpdateVerbose(BanGuard.Instance);
        }
    }

}