using TerrariaApi.Server;
using Terraria;

namespace BanGuard;

[ApiVersion(2, 1)]
public class BanGuard : TerrariaPlugin
{
    public override string Author => "Zyrafaq, Soofa";
    public override string Description => "BanGuard lets Terraria admins share bans and verify accounts via Discord.";
    public override string Name => "BanGuard";
    public override Version Version => new Version(0, 1, 9);
    public BanGuard(Main game) : base(game) { }
    public static TerrariaPlugin Instance { get; private set; } = null!;
    public static Configuration Config = Configuration.Reload();

    public override void Initialize()
    {
        Instance = this;
        Commands.Initialize();
        Handlers.Initialize();
        APIService.Initialize();
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            Handlers.Dispose();
        }
        base.Dispose(disposing);
    }
}
