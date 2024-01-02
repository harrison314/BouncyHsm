using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Pkcs;

internal class ListPkcsObjectCommand : AsyncCommand<ListPkcsObjectCommand.Settings>
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
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);
        PkcsObjectsDto objects = default!;

        await AnsiConsole.Status()
            .StartAsync("Loading...", async ctx =>
            {
                objects = await client.GetPkcsObjectsAsync(settings.SlotId);
            });

        foreach (PkcsObjectInfoDto info in objects.Objects)
        {
            AnsiConsole.MarkupLine("[green]{0}[/]", info.Subject ?? "-");
            Grid grid = new Grid();
            grid.AddColumn().AddColumn();

            grid.AddRow(new string[] { "CkaLabel:", Markup.Escape(info.CkaLabel) });
            grid.AddRow(new string[] { "CkaId:", Markup.Escape(HexConvertorSlim.ToHex(info.CkaId)) });
            grid.AddRow(new string[] { "Always Authenticate:", info.AlwaysAuthenticate? "[green]yes[/]" : "[yellow]no[/]" });
            AnsiConsole.Write(grid);

            Table table = new Table();
            table.AddColumn("Id");
            table.AddColumn("CKO");
            table.AddColumn("Description");

            foreach (PkcsSpecificObjectDto specificObject in info.Objects)
            {
                table.AddRow(new Markup($"[green]{specificObject.ObjectId}[/]"),
                    new Markup(specificObject.CkaClass.ToString()),
                    new Markup(Markup.Escape(specificObject.Description)));
            }

            AnsiConsole.Write(table);
            AnsiConsole.WriteLine();
        }

        return 0;
    }
}
