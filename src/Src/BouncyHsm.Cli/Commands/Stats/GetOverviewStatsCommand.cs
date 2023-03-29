using BouncyHsm.Cli.Commands.Slot;
using BouncyHsm.Spa.Services.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Stats;

internal class GetOverviewStatsCommand : AsyncCommand<GetOverviewStatsCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {

    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        BouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);
        OverviewStatsDto stats = default!;

        await AnsiConsole.Status()
            .StartAsync("Loading...", async ctx =>
            {
                stats = await client.GetOverviewStatsAsync();
            });

        Grid grid = new Grid();
        grid.AddColumn(new GridColumn().RightAligned()).AddColumn();

        grid.AddRow(new string[] { "Slots:", $"[green]{stats.SlotCount}[/]" });
        grid.AddRow(new string[] { "Objects:", $"[green]{stats.TotalObjectCount}[/]" });
        grid.AddRow(new string[] { "Private keys:", $"[green]{stats.PrivateKeys}[/]" });
        grid.AddRow(new string[] { "X509 Certificates:", $"[green]{stats.X509Certificates}[/]" });
        grid.AddRow(new string[] { "Connections:", $"[green]{stats.ConnectedApplications}[/]" });
        grid.AddRow(new string[] { "RO sessions:", $"[green]{stats.RoSessionCount}[/]" });
        grid.AddRow(new string[] { "RW session:", $"[green]{stats.RwSessionCount}[/]" });
        AnsiConsole.Write(grid);

        return 0;
    }
}
