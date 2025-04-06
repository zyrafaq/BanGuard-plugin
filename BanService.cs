using Microsoft.Xna.Framework;
using TShockAPI;
using TShockAPI.DB;
using System.Text;
using System.Security.Cryptography;

namespace BanGuard;

public static class BanService
{
    private static void Help(CommandArgs args)
    {
        if (args.Parameters.Count > 1)
        {
            MoreHelp(args);
            return;
        }

        //TODO: Translate. The string interpolation here wil break the text extractor.
        args.Player.SendMessage("TShock Ban Help", Color.White);
        args.Player.SendMessage("Available Ban commands:", Color.White);
        args.Player.SendMessage($"ban {"add".Color(Utils.RedHighlight)} <Target> [Flags]", Color.White);
        args.Player.SendMessage($"ban {"del".Color(Utils.RedHighlight)} <Ban ID>", Color.White);
        args.Player.SendMessage($"ban {"list".Color(Utils.RedHighlight)}", Color.White);
        args.Player.SendMessage($"ban {"details".Color(Utils.RedHighlight)} <Ban ID>", Color.White);
        args.Player.SendMessage($"Quick usage: {"ban add".Color(Utils.BoldHighlight)} {args.Player.Name.Color(Utils.RedHighlight)} \"Griefing\"", Color.White);
        args.Player.SendMessage($"For more info, use {"ban help".Color(Utils.BoldHighlight)} {"command".Color(Utils.RedHighlight)} or {"ban help".Color(Utils.BoldHighlight)} {"examples".Color(Utils.RedHighlight)}", Color.White);
    }

    private static void MoreHelp(CommandArgs args)
    {
        string cmd = args.Parameters[1].ToLower();
        switch (cmd)
        {
            case "add":
                args.Player.SendMessage("", Color.White);
                args.Player.SendMessage("Ban Add Syntax", Color.White);
                args.Player.SendMessage($"{"ban add".Color(Utils.BoldHighlight)} <{"Target".Color(Utils.RedHighlight)}> [{"Reason".Color(Utils.BoldHighlight)}] [{"Duration".Color(Utils.PinkHighlight)}] [{"Flags".Color(Utils.GreenHighlight)}]", Color.White);
                args.Player.SendMessage($"- {"Duration".Color(Utils.PinkHighlight)}: uses the format {"0d0m0s".Color(Utils.PinkHighlight)} to determine the length of the ban.", Color.White);
                args.Player.SendMessage($"   Eg a value of {"10d30m0s".Color(Utils.PinkHighlight)} would represent 10 days, 30 minutes, 0 seconds.", Color.White);
                args.Player.SendMessage($"   If no duration is provided, the ban will be permanent.", Color.White);
                args.Player.SendMessage($"- {"Flags".Color(Utils.GreenHighlight)}: -a (account name), -u (UUID), -n (character name), -ip (IP address), -e (exact, {"Target".Color(Utils.RedHighlight)} will be treated as identifier)", Color.White);
                args.Player.SendMessage($"   Unless {"-e".Color(Utils.GreenHighlight)} is passed to the command, {"Target".Color(Utils.RedHighlight)} is assumed to be a player or player index", Color.White);
                args.Player.SendMessage($"   If no {"Flags".Color(Utils.GreenHighlight)} are specified, the command uses {"-a -u -ip".Color(Utils.GreenHighlight)} by default.", Color.White);
                args.Player.SendMessage($"Example usage: {"ban add".Color(Utils.BoldHighlight)} {args.Player.Name.Color(Utils.RedHighlight)} {"\"Cheating\"".Color(Utils.BoldHighlight)} {"10d30m0s".Color(Utils.PinkHighlight)} {"-a -u -ip".Color(Utils.GreenHighlight)}", Color.White);
                break;

            case "del":
                args.Player.SendMessage("", Color.White);
                args.Player.SendMessage("Ban Del Syntax", Color.White);
                args.Player.SendMessage($"{"ban del".Color(Utils.BoldHighlight)} <{"Ticket Number".Color(Utils.RedHighlight)}>", Color.White);
                args.Player.SendMessage($"- {"Ticket Numbers".Color(Utils.RedHighlight)} are provided when you add a ban, and can also be viewed with the {"ban list".Color(Utils.BoldHighlight)} command.", Color.White);
                args.Player.SendMessage($"Example usage: {"ban del".Color(Utils.BoldHighlight)} {"12345".Color(Utils.RedHighlight)}", Color.White);
                break;

            case "list":
                args.Player.SendMessage("", Color.White);
                args.Player.SendMessage("Ban List Syntax", Color.White);
                args.Player.SendMessage($"{"ban list".Color(Utils.BoldHighlight)} [{"Page".Color(Utils.PinkHighlight)}]", Color.White);
                args.Player.SendMessage("- Lists active bans. Color trends towards green as the ban approaches expiration", Color.White);
                args.Player.SendMessage($"Example usage: {"ban list".Color(Utils.BoldHighlight)}", Color.White);
                break;

            case "details":
                args.Player.SendMessage("", Color.White);
                args.Player.SendMessage("Ban Details Syntax", Color.White);
                args.Player.SendMessage($"{"ban details".Color(Utils.BoldHighlight)} <{"Ticket Number".Color(Utils.RedHighlight)}>", Color.White);
                args.Player.SendMessage($"- {"Ticket Numbers".Color(Utils.RedHighlight)} are provided when you add a ban, and can be found with the {"ban list".Color(Utils.BoldHighlight)} command.", Color.White);
                args.Player.SendMessage($"Example usage: {"ban details".Color(Utils.BoldHighlight)} {"12345".Color(Utils.RedHighlight)}", Color.White);
                break;

            case "identifiers":
                if (!PaginationTools.TryParsePageNumber(args.Parameters, 2, args.Player, out int pageNumber))
                {
                    args.Player.SendMessage($"Invalid page number. Page number must be numeric.", Color.White);
                    return;
                }

                var idents = from ident in Identifier.Available
                             select $"{ident.Color(Utils.RedHighlight)} - {ident.Description}";

                args.Player.SendMessage((""), Color.White);
                PaginationTools.SendPage(args.Player, pageNumber, idents.ToList(),
                    new PaginationTools.Settings
                    {
                        HeaderFormat = "Available identifiers ({{0}}/{{1}}):",
                        FooterFormat = string.Format("Type {0}ban help identifiers {{0}} for more.", TShockAPI.Commands.Specifier),
                        NothingToDisplayString = "There are currently no available identifiers.",
                        HeaderTextColor = Color.White,
                        LineTextColor = Color.White
                    });
                break;

            case "examples":
                args.Player.SendMessage("", Color.White);
                args.Player.SendMessage("Ban Usage Examples", Color.White);
                args.Player.SendMessage("- Ban an offline player by account name", Color.White);
                args.Player.SendMessage($"   {TShockAPI.Commands.Specifier}{"ban add".Color(Utils.BoldHighlight)} \"{"acc:".Color(Utils.RedHighlight)}{args.Player.Account.Color(Utils.RedHighlight)}\" {"\"Multiple accounts are not allowed\"".Color(Utils.BoldHighlight)} {"-e".Color(Utils.GreenHighlight)} (Permanently bans this account name)", Color.White);
                args.Player.SendMessage("- Ban an offline player by IP address", Color.White);
                args.Player.SendMessage($"   {TShockAPI.Commands.Specifier}{"ai".Color(Utils.BoldHighlight)} \"{args.Player.Account.Color(Utils.RedHighlight)}\" (Find the IP associated with the offline target's account)", Color.White);
                args.Player.SendMessage($"   {TShockAPI.Commands.Specifier}{"ban add".Color(Utils.BoldHighlight)} {"ip:".Color(Utils.RedHighlight)}{args.Player.IP.Color(Utils.RedHighlight)} {"\"Griefing\"".Color(Utils.BoldHighlight)} {"-e".Color(Utils.GreenHighlight)} (Permanently bans this IP address)", Color.White);
                args.Player.SendMessage($"- Ban an online player by index (Useful for hard to type names)", Color.White);
                args.Player.SendMessage($"   {TShockAPI.Commands.Specifier}{"who".Color(Utils.BoldHighlight)} {"-i".Color(Utils.GreenHighlight)} (Find the player index for the target)", Color.White);
                args.Player.SendMessage($"   {TShockAPI.Commands.Specifier}{"ban add".Color(Utils.BoldHighlight)} {"tsi:".Color(Utils.RedHighlight)}{args.Player.Index.Color(Utils.RedHighlight)} {"\"Trolling\"".Color(Utils.BoldHighlight)} {"-a -u -ip".Color(Utils.GreenHighlight)} (Permanently bans the online player by Account, UUID, and IP)", Color.White);
                // Ban by account ID when?
                break;

            default:
                args.Player.SendMessage($"Unknown ban command. Try {"ban help".Color(Utils.BoldHighlight)} {"add".Color(Utils.RedHighlight)}, {"del".Color(Utils.RedHighlight)}, {"list".Color(Utils.RedHighlight)}, {"details".Color(Utils.RedHighlight)}, {"identifiers".Color(Utils.RedHighlight)}, or {"examples".Color(Utils.RedHighlight)}.", Color.White); break;
        }
    }

    private static void DisplayBanDetails(CommandArgs args, Ban ban)
    {
        args.Player.SendMessage(($"{"Ban Details".Color(Utils.BoldHighlight)} - Ticket Number: {ban.TicketNumber.Color(Utils.GreenHighlight)}"), Color.White);
        args.Player.SendMessage(($"{"Identifier:".Color(Utils.BoldHighlight)} {ban.Identifier}"), Color.White);
        args.Player.SendMessage(($"{"Reason:".Color(Utils.BoldHighlight)} {ban.Reason}"), Color.White);
        args.Player.SendMessage(($"{"Banned by:".Color(Utils.BoldHighlight)} {ban.BanningUser.Color(Utils.GreenHighlight)} on {ban.BanDateTime.ToString("yyyy/MM/dd").Color(Utils.RedHighlight)} ({ban.GetPrettyTimeSinceBanString().Color(Utils.YellowHighlight)} ago)"), Color.White);
        if (ban.ExpirationDateTime < DateTime.UtcNow)
        {
            args.Player.SendMessage(($"{"Ban expired:".Color(Utils.BoldHighlight)} {ban.ExpirationDateTime.ToString("yyyy/MM/dd").Color(Utils.RedHighlight)} ({ban.GetPrettyExpirationString().Color(Utils.YellowHighlight)} ago)"), Color.White);
        }
        else
        {
            string remaining;
            if (ban.ExpirationDateTime == DateTime.MaxValue)
            {
                remaining = ("Never.").Color(Utils.YellowHighlight);
            }
            else
            {
                remaining = ($"{ban.GetPrettyExpirationString().Color(Utils.YellowHighlight)} remaining.");
            }

            args.Player.SendMessage(($"{"Ban expires:".Color(Utils.BoldHighlight)} {ban.ExpirationDateTime.ToString("yyyy/MM/dd").Color(Utils.RedHighlight)} ({remaining})"), Color.White);
        }
    }

    private static AddBanResult DoBan(CommandArgs args, string ident, string reason, DateTime expiration)
    {
        AddBanResult banResult = TShock.Bans.InsertBan(ident, reason, args.Player.Account.Name, DateTime.UtcNow, expiration);
        if (banResult.Ban != null)
        {
            args.Player.SendSuccessMessage($"Ban added. Ticket Number {banResult.Ban.TicketNumber.Color(Utils.GreenHighlight)} was created for identifier {ident.Color(Utils.WhiteHighlight)}.");
        }
        else
        {
            args.Player.SendWarningMessage($"Failed to add ban for identifier: {ident.Color(Utils.WhiteHighlight)}.");
            args.Player.SendWarningMessage($"Reason: {banResult.Message}.");
        }

        return banResult;
    }

    private static void AddBan(CommandArgs args)
    {
        if (!args.Parameters.TryGetValue(1, out string target))
        {
            args.Player.SendMessage($"Invalid Ban Add syntax. Refer to {"ban help add".Color(Utils.BoldHighlight)} for details on how to use the {"ban add".Color(Utils.BoldHighlight)} command", Color.White);
            return;
        }

        bool exactTarget = args.Parameters.Any(p => p == "-e");
        bool banAccount = args.Parameters.Any(p => p == "-a");
        bool banUuid = args.Parameters.Any(p => p == "-u");
        bool banName = args.Parameters.Any(p => p == "-n");
        bool banIp = args.Parameters.Any(p => p == "-ip");
        int categoryIndex = args.Parameters.FindIndex(p => p == "-c") + 1;
        string? category = null;

        if (categoryIndex > 0 && categoryIndex < args.Parameters.Count)
        {
            category = args.Parameters[categoryIndex];
        }

        List<string> flags = new List<string>() { "-e", "-a", "-u", "-n", "-ip", "-c" };

        string reason = "Banned.";
        string duration = null;
        DateTime expiration = DateTime.MaxValue;

        //This is hacky. We want flag values to be independent of order so we must force the consecutive ordering of the 'reason' and 'duration' parameters,
        //while still allowing them to be placed arbitrarily in the parameter list.
        //As an example, the following parameter lists (and more) should all be acceptable:
        //-u "reason" -a duration -ip
        //"reason" duration -u -a -ip
        //-u -a -ip "reason" duration
        //-u -a -ip
        for (int i = 2; i < args.Parameters.Count; i++)
        {
            var param = args.Parameters[i];
            if (!flags.Contains(param))
            {
                reason = param;
                break;
            }
        }
        for (int i = 3; i < args.Parameters.Count; i++)
        {
            var param = args.Parameters[i];
            if (!flags.Contains(param))
            {
                duration = param;
                break;
            }
        }

        if (TShock.Utils.TryParseTime(duration, out ulong seconds))
        {
            expiration = DateTime.UtcNow.AddSeconds(seconds);
        }

        //If no flags were specified, default to account, uuid, and IP
        if (!exactTarget && !banAccount && !banUuid && !banName && !banIp)
        {
            banAccount = banUuid = banIp = true;

            if (TShock.Config.Settings.DisableDefaultIPBan)
            {
                banIp = false;
            }
        }

        reason = reason ?? "Banned";

        if (exactTarget)
        {
            DoBan(args, target, reason, expiration);
            return;
        }

        var players = TSPlayer.FindByNameOrID(target);

        if (players.Count > 1)
        {
            args.Player.SendMultipleMatchError(players.Select(p => p.Name));
            return;
        }

        if (players.Count < 1)
        {
            args.Player.SendErrorMessage("Could not find the target specified. Check that you have the correct spelling.");
            return;
        }

        var player = players[0];
        AddBanResult banResult = null;

        if (banAccount)
        {
            if (player.Account != null)
            {
                banResult = DoBan(args, $"{Identifier.Account}{player.Account.Name}", reason, expiration);
            }
        }

        if (banUuid && player.UUID.Length > 0)
        {
            banResult = DoBan(args, $"{Identifier.UUID}{player.UUID}", reason, expiration);
        }

        if (banName)
        {
            banResult = DoBan(args, $"{Identifier.Name}{player.Name}", reason, expiration);
        }

        if (banIp)
        {
            banResult = DoBan(args, $"{Identifier.IP}{player.IP}", reason, expiration);
        }

        if (category != null)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(player.IP);
            byte[] hashBytes = SHA256.HashData(buffer);
            string hash = Convert.ToHexString(hashBytes);
            _ = APIService.BanPlayer(player.UUID, category, hash);
            player.Disconnect(($"You have been banned on the BanGuard network for {category}."));
        }
        else if (banResult?.Ban != null)
        {
            player.Disconnect(($"#{banResult.Ban.TicketNumber} - You have been banned: {banResult.Ban.Reason}."));
        }
    }

    private static void DelBan(CommandArgs args)
    {
        if (!args.Parameters.TryGetValue(1, out string target))
        {
            args.Player.SendMessage($"Invalid Ban Del syntax. Refer to {"ban help del".Color(Utils.BoldHighlight)} for details on how to use the {"ban del".Color(Utils.BoldHighlight)} command", Color.White);
            return;
        }

        if (!int.TryParse(target, out int banId))
        {
            args.Player.SendMessage($"Invalid Ticket Number. Refer to {"ban help del".Color(Utils.BoldHighlight)} for details on how to use the {"ban del".Color(Utils.BoldHighlight)} command", Color.White);
            return;
        }

        if (TShock.Bans.RemoveBan(banId))
        {
            TShock.Log.ConsoleInfo($"Ban {banId} has been revoked by {args.Player.Account.Name}.");
            args.Player.SendSuccessMessage($"Ban {banId.Color(Utils.GreenHighlight)} has now been marked as expired.");
        }
        else
        {
            args.Player.SendErrorMessage("Failed to remove ban.");
        }
    }

    private static void ListBans(CommandArgs args)
    {
        string PickColorForBan(Ban ban)
        {
            double hoursRemaining = (ban.ExpirationDateTime - DateTime.UtcNow).TotalHours;
            double hoursTotal = (ban.ExpirationDateTime - ban.BanDateTime).TotalHours;
            double percentRemaining = TShock.Utils.Clamp(hoursRemaining / hoursTotal, 100, 0);

            int red = TShock.Utils.Clamp((int)(255 * 2.0f * percentRemaining), 255, 0);
            int green = TShock.Utils.Clamp((int)(255 * (2.0f * (1 - percentRemaining))), 255, 0);

            return $"{red:X2}{green:X2}{0:X2}";
        }

        if (!PaginationTools.TryParsePageNumber(args.Parameters, 1, args.Player, out int pageNumber))
        {
            args.Player.SendMessage(($"Invalid Ban List syntax. Refer to {"ban help list".Color(Utils.BoldHighlight)} for details on how to use the {"ban list".Color(Utils.BoldHighlight)} command"), Color.White);
            return;
        }

        var bans = from ban in TShock.Bans.Bans
                   where ban.Value.ExpirationDateTime > DateTime.UtcNow
                   orderby ban.Value.ExpirationDateTime ascending
                   select $"[{ban.Key.Color(Utils.GreenHighlight)}] {ban.Value.Identifier.Color(PickColorForBan(ban.Value))}";

        PaginationTools.SendPage(args.Player, pageNumber, bans.ToList(),
            new PaginationTools.Settings
            {
                HeaderFormat = ("Bans ({{0}}/{{1}}):"),
                FooterFormat = string.Format("Type {0}ban list {{0}} for more.", TShockAPI.Commands.Specifier),
                NothingToDisplayString = ("There are currently no active bans.")
            });
    }

    private static void BanDetails(CommandArgs args)
    {
        if (!args.Parameters.TryGetValue(1, out string target))
        {
            args.Player.SendMessage(($"Invalid Ban Details syntax. Refer to {"ban help details".Color(Utils.BoldHighlight)} for details on how to use the {"ban details".Color(Utils.BoldHighlight)} command"), Color.White);
            return;
        }

        if (!int.TryParse(target, out int banId))
        {
            args.Player.SendMessage(($"Invalid Ticket Number. Refer to {"ban help details".Color(Utils.BoldHighlight)} for details on how to use the {"ban details".Color(Utils.BoldHighlight)} command"), Color.White);
            return;
        }

        Ban ban = TShock.Bans.GetBanById(banId);

        if (ban == null)
        {
            args.Player.SendErrorMessage(("No bans found matching the provided ticket number."));
            return;
        }

        DisplayBanDetails(args, ban);
    }

    public static void Ban(CommandArgs args)
    {
        //Ban syntax:
        // ban add <target> [reason] [duration] [flags (default: -a -u -ip)]
        //						Duration is in the format 0d0h0m0s. Any part can be ignored. E.g., 1s is a valid ban time, as is 1d1s, etc. If no duration is specified, ban is permanent
        //						Valid flags: -a (ban account name), -u (ban UUID), -n (ban character name), -ip (ban IP address), -e (exact, ban the identifier provided as 'target')
        //						Unless -e is passed to the command, <target> is assumed to be a player or player index.
        // ban del <ban ID>
        //						Target is expected to be a ban Unique ID
        // ban list [page]
        //						Displays a paginated list of bans
        // ban details <ban ID>
        //						Target is expected to be a ban Unique ID
        //ban help [command]
        //						Provides extended help on specific ban commands


        string subcmd = args.Parameters.Count == 0 ? "help" : args.Parameters[0].ToLower();
        switch (subcmd)
        {
            case "help":
                Help(args);
                break;

            case "add":
                AddBan(args);
                break;

            case "del":
                DelBan(args);
                break;

            case "list":
                ListBans(args);
                break;

            case "details":
                BanDetails(args);
                break;

            default:
                break;
        }
    }
}