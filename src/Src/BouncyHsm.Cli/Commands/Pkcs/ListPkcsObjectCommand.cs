using BouncyHsm.Cli.Commands.Objects;
using BouncyHsm.Spa.Services.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class ListPkcsObjectCommand : AsyncCommand<ListPkcsObjectCommand.Settings>
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
        //BouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);
        //PkcsObjectsDto objects = default!;

        //await AnsiConsole.Status()
        //    .StartAsync("Loading...", async ctx =>
        //    {
        //        objects = await client.GetPkcsObjectsAsync(settings.SlotId);
        //    });

        //Table table = new Table();
        //table.AddColumn("Id");
        //table.AddColumn("Description");
        //table.AddColumn("Token Label");
        //table.AddColumn("Token SerialNumber");
        //table.AddColumn("With HW RNG");
        //table.AddColumn("With QualifiedArea");

        //foreach (PkcsObjectInfoDto info in objects.Objects)
        //{
        //    table.AddRow(new Markup($"[green]{}[/]"),
        //        new Markup(Markup.Escape(slot.Description)),
        //        new Markup(Markup.Escape(slot.Token!.Label)),
        //        new Markup(Markup.Escape(slot.Token!.SerialNumber)),
        //        new Markup(slot.Token!.SimulateHwRng ? "[green]yes[/]" : "[yellow]no[/]"),
        //        new Markup(slot.Token!.SimulateQualifiedArea ? "[green]yes[/]" : "[yellow]no[/]"));
        //}

        //AnsiConsole.Write(table);
        return 0;
    }
}
