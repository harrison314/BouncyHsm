using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System.ComponentModel;

namespace BouncyHsm.Cli.Commands.Stats.AppConnections;

internal class RemoveAppConnectionsCommand : AsyncCommand<RemoveAppConnectionsCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
        [CommandArgument(0, "[AppConnectionId]")]
        public Guid AppSessionId
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
            if (!AnsiConsole.Confirm($"Do you really want to remove the application connection [green]{settings.AppSessionId}[/]?"))
            {
                return 0;
            }
        }

        await AnsiConsole.Status()
               .StartAsync("Removing...", async ctx =>
               {
                   await client.RemoveApplicationConnectionAsync(settings.AppSessionId);
               });

        AnsiConsole.MarkupLine("Application connection with id [green]{0}[/] has removed.", settings.AppSessionId);
        return 0;
    }
}