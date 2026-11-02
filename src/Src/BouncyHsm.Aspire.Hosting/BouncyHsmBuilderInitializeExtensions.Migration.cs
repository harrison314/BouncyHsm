using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Aspire.Hosting;

public static partial class BouncyHsmBuilderInitializeExtensions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithDbMigration(this IResourceBuilder<BouncyHsmResource> builder)
    {
        ArgumentNullException.ThrowIfNull(builder);

        builder.Resource.InitializeTasks.Add(async (client, ct) =>
        {
            await client.MigrateAsync(new BouncyHsm.Client.MigrationRequestDto()
            {
                ResetAllowedMechanism = false
            }, ct);
        });

        return builder;
    }
}
