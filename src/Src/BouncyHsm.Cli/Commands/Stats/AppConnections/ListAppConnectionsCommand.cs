using BouncyHsm.Cli.Commands.Pkcs;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Stats.AppConnections;

internal class ListAppConnectionsCommand : AsyncCommand<ListAppConnectionsCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        Spa.Services.Client.BouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);
        IList<Spa.Services.Client.ApplicationSessionDto> sessions = default!;

        await AnsiConsole.Status()
            .StartAsync("Loading...", async ctx =>
            {
                sessions = await client.GetApplicationConnectionsAsync();
            });

        Table table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Machine");
        table.AddColumn("Application");
        table.AddColumn("PID");
        table.AddColumn("Connected at");
        table.AddColumn("Last interaction");

        foreach (Spa.Services.Client.ApplicationSessionDto session in sessions)
        {
            table.AddRow(new Markup($"[green]{session.ApplicationSessionId}[/]"),
                new Markup(Markup.Escape(session.CompiuterName)),
                new Markup(Markup.Escape(session.ApplicationName)),
                new Markup(Markup.Escape(session.Pid.ToString())),
                new Markup(Markup.Escape(session.StartAt.ToString())),
                new Markup(Markup.Escape(session.LastInteraction.ToString())));
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
