using BouncyHsm.Spa.Services.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class ImportCertificateCommand : AsyncCommand<ImportCertificateCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[SlotId]")]
        [Description("Slot Id.")]
        public int SlotId
        {
            get;
            set;
        }

        [CommandArgument(1, "[PrivateKeyId]")]
        [Description("Private key id.")]
        public Guid PrivateKeyId
        {
            get;
            set;
        }

        [CommandArgument(2, "[CertPath]")]
        [Description("Path to X509 certificate file.")]
        public string CertPath
        {
            get;
            set;
        } = default!;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        BouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        if (!File.Exists(settings.CertPath))
        {
            throw new InvalidDataException($"File {settings.CertPath} not found.");
        }

        byte[] certContent = File.ReadAllBytes(settings.CertPath);

        await AnsiConsole.Status()
           .StartAsync("Importing...", async ctx =>
           {
               await client.ImportX509CertificateAsync(settings.SlotId, new ImportX509CertificateRequestDto()
               {
                   PrivateKeyId = settings.PrivateKeyId,
                   Certificate = certContent
               });
           });

        AnsiConsole.MarkupLine("Certificate has imported successfully.");
        return 0;
    }
}