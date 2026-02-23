using BouncyHsm.Client;
using Spectre.Console;
using Spectre.Console.Cli;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace BouncyHsm.Cli.Commands.Migration;

internal class MigrateObjectsCommand : AsyncCommand<MigrateObjectsCommand.Settings>
{
    internal sealed class Settings : BaseSettings
    {
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings, CancellationToken cancellationToken)
    {
        IBouncyHsmClient client = BouncyHsmClientFactory.Create(settings.Endpoint);

        MigrationResultDto? migrationResult = null;
        await AnsiConsole.Status()
           .StartAsync("Migrating objects...", async ctx =>
           {
               migrationResult = await client.MigrateAsync(cancellationToken);
           });

        Debug.Assert(migrationResult is not null);

        if (migrationResult.FailedObjects > 0)
        {
            AnsiConsole.MarkupLine($"Migration completed. [green]{migrationResult.SuccessedObjects}[/] objects are compliant with the current version of BouncyHsm. An error occurred at [red]{migrationResult.FailedObjects}[/].");
        }
        else
        {
            AnsiConsole.MarkupLine($"Migration completed. [green]{migrationResult.SuccessedObjects}[/] objects are compliant with the current version of BouncyHsm. An error occurred at [green]{migrationResult.FailedObjects}[/].");

        }

        return 0;
    }
}
