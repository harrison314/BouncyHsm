using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Slot;

internal class DeleteSlotCommand : AsyncCommand<DeleteSlotCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[SlotId]")]
        public int SlotId
        {
            get;
            set;
        }

        [CommandOption("-y")]
        [DefaultValue(true)]
        public bool Confirm
        {
            get;
            init;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        if (!settings.Confirm)
        {
            SlotDto slot = default!;
            await AnsiConsole.Status()
               .StartAsync("Loading...", async ctx =>
               {
                   slot = await client.GetSlotAsync(settings.SlotId);
               });

            if (!AnsiConsole.Confirm($"Do you really want to delete the slot [green]{settings.SlotId}[/] with token label '[green]{slot.Token.Label}[/]' and serial '[green]{slot.Token.SerialNumber}[/]'?"))
            {
                return 0;
            }
        }

        await AnsiConsole.Status()
               .StartAsync("Deleting...", async ctx =>
               {
                    await client.DeleteSlotAsync(settings.SlotId);
               });

        AnsiConsole.MarkupLine("Slot with id [green]{0}[/] has deleted.", settings.SlotId);
        return 0;
    }
}