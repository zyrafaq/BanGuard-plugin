using BanGuard.Models;
using TShockAPI;

namespace BanGuard;

public static class Commands
{
    public static void Initialize()
    {
        TShockAPI.Commands.ChatCommands.Add(new Command("banguard.listnames", GetPlayerNamesCmd, "getplayernames", "getplayeralts", "getalts")
        {
            HelpText = "Generates a connection code for linking your account."
        });
    }

    public static async void GetPlayerNamesCmd(CommandArgs args)
    {
        if (args.Parameters.Count == 0)
        {
            args.Player.SendErrorMessage("Please provide a player name.");
            return;
        }

        string accountName = args.Parameters[0];

        var account = TShock.UserAccounts.GetUserAccountByName(accountName);
        if (account == null)
        {
            args.Player.SendErrorMessage("Account not found.");
            return;
        }

        List<string> playerNames = await APIService.GetPlayerNames(account.UUID);

        if (playerNames == null)
        {
            args.Player.SendErrorMessage("Error getting player names.");
            return;
        }

        args.Player.SendSuccessMessage($"Player names for {accountName}({playerNames.Count()}):");
        foreach (string name in playerNames)
        {
            args.Player.SendSuccessMessage($"- {name}");
        }
    }
}
