using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;

namespace BouncyHsm.Cli.Commands.Slot;

internal class PlugTokenCommand : AsyncCommand<PlugTokenCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[SlotId]")]
        public int SlotId
        {
            get;
            set;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        await AnsiConsole.Status()
               .StartAsync("Plugging token...", async ctx =>
               {
                   await client.SetSlotPluggedStateAsync(settings.SlotId, new SetPluggedStateDto()
                   {
                       Plugged = false
                   });
               });

        AnsiConsole.MarkupLine("Token is plugged from slot with id [green]{0}[/].", settings.SlotId);
        return 0;
    }
}
