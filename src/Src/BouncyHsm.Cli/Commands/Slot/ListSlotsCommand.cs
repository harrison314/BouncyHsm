using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Slot;

internal class ListSlotsCommand : AsyncCommand<ListSlotsCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {

    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);
        IList<SlotDto> slots = default!;

        await AnsiConsole.Status()
            .StartAsync("Loading...", async ctx =>
            {
                slots = await client.GetAllSlotsAsync();
            });

        Table table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Description");
        table.AddColumn("Token Label");
        table.AddColumn("Token SerialNumber");
        table.AddColumn("With HW RNG");
        table.AddColumn("With Qualified Area");
        table.AddColumn("Plugged token");

        foreach (SlotDto slot in slots)
        {
            table.AddRow(new Markup($"[green]{slot.SlotId}[/]"),
                new Markup(Markup.Escape(slot.Description)),
                new Markup(Markup.Escape(slot.Token.Label)),
                new Markup(Markup.Escape(slot.Token.SerialNumber)),
                new Markup(slot.Token.SimulateHwRng ? "[green]yes[/]" : "[yellow]no[/]"),
                new Markup(slot.Token.SimulateQualifiedArea ? "[green]yes[/]" : "[yellow]no[/]"),
                new Markup(this.FormatPluggedColumn(slot)));
        }

        AnsiConsole.Write(table);
        return 0;
    }

    private string FormatPluggedColumn(SlotDto slot)
    {
        if (!slot.IsRemovableDevice)
        {
            return "[grey]alwais[/]";
        }

        return slot.IsUnplugged ? "[red]no[/]" : "[green]yes[/]";
    }
}
