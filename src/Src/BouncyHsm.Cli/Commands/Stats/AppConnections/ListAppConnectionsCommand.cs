using BouncyHsm.Cli.Commands.Pkcs;
using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Cli.Commands.Stats.AppConnections;

internal class ListAppConnectionsCommand : AsyncCommand<ListAppConnectionsCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandOption("-i|--includeCmdLine")]
        [DefaultValue(false)]
        public bool IncludeCmd
        {
            get;
            init;
        }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);
        IList<ApplicationSessionDto> sessions = default!;

        await AnsiConsole.Status()
            .StartAsync("Loading...", async ctx =>
            {
                sessions = await client.GetApplicationConnectionsAsync(cancellationToken);
            });

        Table table = new Table();
        table.AddColumn("Id");
        table.AddColumn("Machine");
        table.AddColumn("Application");
        table.AddColumn("PID");
        table.AddColumn("Connected at");
        table.AddColumn("Last interaction");

        if (settings.IncludeCmd)
        {
            table.AddColumn("CMD line");
        }

        foreach (ApplicationSessionDto session in sessions)
        {
            if (settings.IncludeCmd)
            {
                table.AddRow(new Markup($"[green]{session.ApplicationSessionId}[/]"),
                    new Markup(Markup.Escape(session.ComputerName)),
                    new Markup(Markup.Escape(session.ApplicationName)),
                    new Markup(Markup.Escape(session.Pid.ToString())),
                    new Markup(Markup.Escape(session.StartAt.ToString())),
                    new Markup(Markup.Escape(session.LastInteraction.ToString())));
            }
            else
            {
                table.AddRow(new Markup($"[green]{session.ApplicationSessionId}[/]"),
                    new Markup(Markup.Escape(session.ComputerName)),
                    new Markup(Markup.Escape(session.ApplicationName)),
                    new Markup(Markup.Escape(session.Pid.ToString())),
                    new Markup(Markup.Escape(session.StartAt.ToString())),
                    new Markup(Markup.Escape(session.LastInteraction.ToString())),
                    new Markup(Markup.Escape(string.Join(Environment.NewLine, session.CmdArguments))));
            }
        }

        AnsiConsole.Write(table);
        return 0;
    }
}
