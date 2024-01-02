using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class GenerateCsrCommand : AsyncCommand<GenerateCsrCommand.Settings>
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

        [CommandArgument(2, "[PublicKeyId]")]
        [Description("Public key id.")]
        public Guid PublicKeyId
        {
            get;
            set;
        }

        [CommandArgument(3, "[SubjectName]")]
        [Description("Text represents X509 subject name. (eg. CN=Test cert, C=SK, 2.5.4.7=MyCity)")]
        public string? SubjectName
        {
            get;
            set;
        }

        [CommandOption("-o|--outputPath <OutputPath>")]
        [Description("Path for store CSR file (*.csr).")]
        public string OutputPath
        {
            get;
            set;
        } = default!;
    }

    public override async Task<int> ExecuteAsync(CommandContext context, GenerateCsrCommand.Settings settings)
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
               Pkcs10Dto result = await client.Pkcs_GetPkcsObjectsAsync(settings.SlotId, new GeneratePkcs10RequestDto()
               {
                   PrivateKeyId = settings.PrivateKeyId,
                   PublicKeyId = settings.PublicKeyId,
                   Subject = new SubjectNameDto()
                   {
                       DirName = subjectName
                   }
               });

               await File.WriteAllBytesAsync(settings.OutputPath, result.Content);
           });

        AnsiConsole.MarkupLine("CSR request stored.");
        return 0;
    }
}