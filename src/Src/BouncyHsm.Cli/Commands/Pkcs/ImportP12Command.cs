using Spectre.Console.Cli;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class ImportP12Command : AsyncCommand<ImportP12Command.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[SlotId]")]
        public int SlotId
        {
            get;
            set;
        }

        [CommandArgument(1, "[CkaLabel]")]
        public string CkaLabel
        {
            get;
            set;
        } = default!;

        [CommandArgument(2, "[CkaId]")]
        public string CkaId
        {
            get;
            set;
        } = default!;

        [CommandArgument(3, "[P12FilePath]")]
        public string P12FilePath
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
