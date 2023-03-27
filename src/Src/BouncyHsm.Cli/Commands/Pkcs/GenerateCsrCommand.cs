using Spectre.Console.Cli;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class GenerateCsrCommand : AsyncCommand<GenerateCsrCommand.Settings>
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

        [CommandArgument(2, "SubjectName")]
        public string? SubjectName
        {
            get;
            set;
        }
    }

    public override Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        throw new NotImplementedException();
    }
}