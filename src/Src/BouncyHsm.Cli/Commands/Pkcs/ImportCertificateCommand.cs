using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class ImportCertificateCommand : AsyncCommand<ImportCertificateCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[SlotId]")]
        [Description("Slot Id.")]
        public required int SlotId
        {
            get;
            init;
        }

        [CommandArgument(1, "[PrivateKeyId]")]
        [Description("Private key id.")]
        public required Guid PrivateKeyId
        {
            get;
            init;
        }

        [CommandArgument(2, "[CertPath]")]
        [Description("Path to X509 certificate file.")]
        public required string CertPath
        {
            get;
            init;
        }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

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
               }, cancellationToken);
           });

        AnsiConsole.MarkupLine("Certificate has imported successfully.");
        return 0;
    }
}