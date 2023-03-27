using Spectre.Console.Cli;

namespace BouncyHsm.Cli.Commands.Slot;

internal class DeleteSlotCommand : AsyncCommand<DeleteSlotCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "SlotId")]
        public int SlotId
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