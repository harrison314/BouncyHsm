using Aspire.Hosting;
using Aspire.Hosting.ApplicationModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Aspire.Hosting.Dangerous;

public static class DangerousExtsnions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithBadHsmLevel(this IResourceBuilder<BouncyHsmResource> builder, BouncyHsmBadHsmLevel level)
    {
        ArgumentNullException.ThrowIfNull(builder);

        if (level != BouncyHsmBadHsmLevel.None)
        {
            string value = level switch
            {
                BouncyHsmBadHsmLevel.Level_1 => "1",
                BouncyHsmBadHsmLevel.Level_2 => "2",
                _ => throw new InvalidProgramException($"Enum value {level} is not supported.")
            };

            return builder.WithEnvironment("BOUNCYHSM_BADHSM_LEVEL", value);
        }
        else
        {
            return builder;
        }
    }
}
