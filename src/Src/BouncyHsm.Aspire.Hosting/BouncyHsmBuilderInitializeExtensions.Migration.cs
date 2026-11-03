using Aspire.Hosting.ApplicationModel;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace Aspire.Hosting;

public static partial class BouncyHsmBuilderInitializeExtensions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithDbMigration(this IResourceBuilder<BouncyHsmResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Resource.InitializeTasks.Add(async (client, logger, ct) =>
        {
            await client.MigrateAsync(new BouncyHsm.Client.MigrationRequestDto()
            {
                ResetAllowedMechanism = false
            }, ct);

            logger.LogInformation("The BouncyHsm database migration has been completed.");
        });

        return builder;
    }
}
