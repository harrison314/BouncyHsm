using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Client;

namespace Aspire.Hosting;

public static partial class BouncyHsmBuilderInitializeExtensions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithInitializedAction(this IResourceBuilder<BouncyHsmResource> builder, BouncyHsmInitializeTask initializeAction)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(initializeAction);

        builder.Resource.InitializeTasks.Add(initializeAction);

        return builder;
    }
}