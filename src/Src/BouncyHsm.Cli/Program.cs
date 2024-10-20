using BouncyHsm.Cli.Commands.Objects;
using BouncyHsm.Cli.Commands.Pkcs;
using BouncyHsm.Cli.Commands.Slot;
using BouncyHsm.Cli.Commands.Stats;
using BouncyHsm.Cli.Commands.Stats.AppConnections;
using Spectre.Console.Cli;

namespace BouncyHsm.Cli;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        CommandApp app = new CommandApp();
        app.Configure(config =>
        {
            config.SetApplicationName("BouncyHsm.Cli");
            config.AddBranch("slot", slot =>
            {
                slot.AddCommand<ListSlotsCommand>("list").WithDescription("List all slots and tokens.");
                slot.AddCommand<CreateSlotCommand>("create").WithDescription("Create a new slot with token.");
                slot.AddCommand<DeleteSlotCommand>("delete").WithDescription("Delete slot with token.");
                slot.AddCommand<PlugTokenCommand>("plug").WithDescription("Plug token into slot.");
                slot.AddCommand<UnplugTokenCommand>("unplug").WithDescription("Unplug token from slot.");
                slot.AddCommand<SetPinCommand>("setPin").WithDescription("Set PIN for token.");

                slot.SetDescription("Slot manipulation.");
            });

            config.AddBranch("objects", slot =>
            {
                slot.AddCommand<ListObjectsCommand>("list").WithDescription("List all objects in token.");

                slot.SetDescription("Crypto objects manipulation.");
            });

            config.AddBranch("pkcs", pkcs =>
            {
                pkcs.AddCommand<ListPkcsObjectCommand>("list").WithDescription("List all objects in token.");
                pkcs.AddCommand<ImportP12Command>("importP12").WithDescription("Import P12/PFX file into token.");
                pkcs.AddCommand<GenerateCsrCommand>("generateCsr").WithDescription("Generate CSR (PKCS#10 request).");
                pkcs.AddCommand<ImportCertificateCommand>("importCert").WithDescription("Import certificate file into token.");

                pkcs.AddBranch("generate", generate =>
                {
                    generate.AddCommand<GenerateRsaKeyPairCommand>("rsa").WithDescription("Generate RSA key pair.");
                    generate.AddCommand<GenerateEcKeyPairCommand>("ec").WithDescription("Generate EC key pair.");
                    generate.AddCommand<GenerateAesKeyCommand>("aes").WithDescription("Generate AES key.");
                    generate.AddCommand<GenerateSecretKeyCommand>("secret").WithDescription("Generate secret key.");

                    generate.SetDescription("Generating keys.");
                });

                pkcs.SetDescription("PKCS objects manipulation.");
            });

            config.AddBranch("appConnections", stats =>
            {
                stats.AddCommand<ListAppConnectionsCommand>("list").WithDescription("Display application connections.");
                stats.AddCommand<RemoveAppConnectionsCommand>("remove").WithDescription("Remove application connection.");

                stats.SetDescription("Manage application connections.");
            });

            config.AddBranch("stats", stats =>
            {
                stats.AddCommand<GetOverviewStatsCommand>("overview").WithDescription("Get overview stats.");

                stats.SetDescription("Display of statistical data.");
            });
        });

        return await app.RunAsync(args);
    }
}