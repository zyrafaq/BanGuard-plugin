using TShockAPI;

namespace BanGuard;

public static class Commands
{
    private static bool _dcCommandsInitialized = false;
    private static bool _banCommandReplaced = false;

    public static void Initialize()
    {
        if (BanGuard.Config.EnableDiscordConnection && !_dcCommandsInitialized)
        {
            TShockAPI.Commands.ChatCommands.Add(new Command("banguard.connect", ConnectCmd, "connect", "link")
            {
                AllowServer = false,
                HelpText = "Generates a connection code for linking your account."
            });

            TShockAPI.Commands.ChatCommands.Add(new Command("banguard.checkconnection", CheckConnectionCmd, "checkconnection")
            {
                AllowServer = false,
                HelpText = "Checks if your account is linked."
            });

            _dcCommandsInitialized = true;
        }
        else if (!BanGuard.Config.EnableDiscordConnection && _dcCommandsInitialized)
        {
            Command? dcConnectCmd = TShockAPI.Commands.ChatCommands.Find(c => c.Name == "connect");
            Command? dcCheckCmd = TShockAPI.Commands.ChatCommands.Find(c => c.Name == "checkconnection");

            if (dcConnectCmd != null)
            {
                Console.WriteLine($"deletin success: {TShockAPI.Commands.ChatCommands.Remove(dcConnectCmd)}");
            }
            if (dcCheckCmd != null)
            {
                TShockAPI.Commands.ChatCommands.Remove(dcCheckCmd);
            }

            _dcCommandsInitialized = false;
        }

        if (!_banCommandReplaced)
        {
            Command? banCmd = TShockAPI.Commands.ChatCommands.Find(c => c.Name == "ban");

            if (banCmd != null)
            {
                banCmd.CommandDelegate = BanService.Ban;
            }
        }
    }

    private static async void ConnectCmd(CommandArgs args)
    {
        if (args.Player.GetDiscordAccount() != null)
        {
            args.Player.SendErrorMessage("You are already linked to an account.");
            return;
        }

        int? code = await APIService.GenerateNewConnection(args.Player.UUID);

        if (code != null)
        {
            args.Player.SendSuccessMessage($"Your connection code is: {code}\n" +
                "Go to https://banguard.uk/link/ to link your account.\n" +
                "After completing the linking process, you must rejoin the server."
            );
        }
        else
        {
            args.Player.SendErrorMessage("Failed to generate connection code.");
        }
    }

    private static async void CheckConnectionCmd(CommandArgs args)
    {
        DCAccount? acc = args.Player.GetDiscordAccount() ?? await APIService.TryGetDiscordAccount(args.Player.UUID);

        if (acc == null)
        {
            args.Player.SendErrorMessage("You are not linked to any account.");
            return;
        }

        args.Player.SetDiscordAccount(acc);
        args.Player.SendSuccessMessage($"You are linked to {acc.DiscordName} on Discord.");
    }
}
