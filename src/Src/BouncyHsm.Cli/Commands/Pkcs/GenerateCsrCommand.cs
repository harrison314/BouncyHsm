using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class GenerateCsrCommand : AsyncCommand<GenerateCsrCommand.Settings>
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

        [CommandArgument(2, "[PublicKeyId]")]
        [Description("Public key id.")]
        public required Guid PublicKeyId
        {
            get;
            init;
        }

        [CommandArgument(3, "[SubjectName]")]
        [Description("Text represents X509 subject name. (eg. CN=Test cert, C=SK, 2.5.4.7=MyCity)")]
        public string? SubjectName
        {
            get;
            init;
        }

        [CommandOption("-o|--outputPath <OutputPath>", isRequired: true)]
        [Description("Path for store CSR file (*.csr).")]
        public required string OutputPath
        {
            get;
            init;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GenerateCsrCommand.Settings settings, CancellationToken cancellationToken)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        string subjectName;
        if (string.IsNullOrEmpty(settings.SubjectName))
        {
            subjectName = AnsiConsole.Prompt(new TextPrompt<string>("Enter subject name:"));
        }
        else
        {
            subjectName = settings.SubjectName;
        }

        await AnsiConsole.Status()
           .StartAsync("Creating...", async ctx =>
           {
               Pkcs10Dto result = await client.Pkcs_GeneratePkcs10Async(settings.SlotId, new GeneratePkcs10RequestDto()
               {
                   PrivateKeyId = settings.PrivateKeyId,
                   PublicKeyId = settings.PublicKeyId,
                   Subject = new SubjectNameDto()
                   {
                       DirName = subjectName
                   }
               },
               cancellationToken);

               await File.WriteAllBytesAsync(settings.OutputPath, result.Content, cancellationToken);
           });

        AnsiConsole.MarkupLine("CSR request stored.");
        return 0;
    }
}