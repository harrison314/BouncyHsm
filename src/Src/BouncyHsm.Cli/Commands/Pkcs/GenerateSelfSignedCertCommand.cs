using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class GenerateSelfSignedCertCommand : AsyncCommand<GenerateSelfSignedCertCommand.Settings>
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

        [CommandOption("-d|--signatureDigestHint <SignatureDigestHint>", isRequired: false)]
        [Description("The digest algorithm is used for signing only if the key type allows it.")]
        [DefaultValue(PkcsDigestAlgorithm.SHA256)]
        public required PkcsDigestAlgorithm DigestHint
        {
            get;
            init;
        }

        [CommandOption("-v|--validity <ValidityInDays>", isRequired: false)]
        [Description("Certificate validity in days.")]
        [DefaultValue(365)]
        public required int Validity
        {
            get;
            init;
        }
    }

    protected override async Task<int> ExecuteAsync(CommandContext context, GenerateSelfSignedCertCommand.Settings settings, CancellationToken cancellationToken)
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
               _ = await client.Pkcs_GenerateSelfSignedCertAsync(settings.SlotId, new GenerateSelfSignedCertRequestDto()
               {
                   PrivateKeyId = settings.PrivateKeyId,
                   PublicKeyId = settings.PublicKeyId,
                   Validity = TimeSpan.FromDays(settings.Validity),
                   SignatureDigestHint = settings.DigestHint,
                   Subject = new SubjectNameDto()
                   {
                       DirName = subjectName
                   }
               },
               cancellationToken);

           });

        AnsiConsole.MarkupLine("X509 certificate created.");
        return 0;
    }
}