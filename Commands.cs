using TShockAPI;

namespace BanGuard;

public static class Commands
{
    public static void Initialize()
    {
        TShockAPI.Commands.ChatCommands.Add(new Command("banguard.connect", ConnectCmd, "connect", "link")
        {
            AllowServer = false,
            HelpText = "Generates a connection code for linking your account."
        });

        Command? banCmd = TShockAPI.Commands.ChatCommands.Find(c => c.Name == "ban");

        if (banCmd != null)
        {
            banCmd.CommandDelegate = BanService.Ban;
        }
    }

    private static async void ConnectCmd(CommandArgs args)
    {
        int? code = await APIService.GenerateNewConnection(args.Player.UUID, args.Player.Name);

        if (code != null)
        {
            args.Player.SendSuccessMessage($"Your connection code is: {code}\nGo to https://banguard.uk/link/ to link your account.");
        }
        else
        {
            args.Player.SendErrorMessage("Failed to generate connection code.");
        }
    }
}
