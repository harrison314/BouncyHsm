using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class ImportP12Command : AsyncCommand<ImportP12Command.Settings>
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

        [CommandArgument(1, "[CkaLabel]")]
        [Description("CkaLabel.")]
        public string CkaLabel
        {
            get;
            set;
        } = default!;

        [CommandArgument(2, "[CkaId]")]
        [Description("CkaId with prefix 'utf8:' for text representation, 'hex:' for hexadecimal representation and 'base64:' for base64 representation.")]
        public string CkaId
        {
            get;
            set;
        } = default!;

        [CommandArgument(3, "[P12FilePath]")]
        [Description("Path to P12/PFX file.")]
        public string P12FilePath
        {
            get;
            set;
        } = default!;

        [CommandOption("-p|--password <Password>")]
        [DefaultValue(null)]
        [Description("Password for P12/PFX file. (Optional parameter)")]
        public string? Password
        {
            get;
            set;
        }

        [CommandOption("-m|--importMode <ImportMode>")]
        [DefaultValue(PrivateKeyImportMode.Imported)]
        [Description("Import mode.  Allowed values are Imported, Local and LocalInQualifiedArea. (Optional parameter)")]
        public PrivateKeyImportMode ImportMode
        {
            get;
            set;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);
        string password;
        if (string.IsNullOrEmpty(settings.Password))
        {
            string p12FileName = Markup.Escape(System.IO.Path.GetFileName(settings.P12FilePath));
            password = AnsiConsole.Prompt(new TextPrompt<string>($"Enter password for [green]{p12FileName}[/]:").Secret());
        }
        else
        {
            password = settings.Password;
        }

        byte[] ckaId = CkaIdParser.Parse(settings.CkaId);
        if (!File.Exists(settings.P12FilePath))
        {
            throw new InvalidDataException($"File {settings.P12FilePath} not found.");
        }

        byte[] p12Content = File.ReadAllBytes(settings.P12FilePath);

        await AnsiConsole.Status()
            .StartAsync("Importing...", async ctx =>
            {
                await client.ImportP12Async(settings.SlotId, new ImportP12RequestDto()
                {
                    CkaId = ckaId,
                    CkaLabel = settings.CkaLabel,
                    ImportChain = false,
                    ImportMode = settings.ImportMode,
                    Password = password,
                    Pkcs12Content = p12Content
                });
            });

        AnsiConsole.MarkupLine("P12 file has imported successfully.");
        return 0;
    }
}
