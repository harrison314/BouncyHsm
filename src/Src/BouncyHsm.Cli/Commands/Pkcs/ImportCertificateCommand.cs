using Spectre.Console.Cli;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class ImportCertificateCommand : AsyncCommand<ImportCertificateCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "SlotId")]
        public int SlotId
        {
            get;
            set;
        }

        [CommandArgument(1, "PrivateKeyId")]
        public Guid PrivateKeyId
        {
            get;
            set;
        }

        [CommandArgument(2, "Certpath")]
        public string Certpath
        {
            get;
            set;
        } = default!;
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        throw new NotImplementedException();
    }
}