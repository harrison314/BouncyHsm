using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Client;

namespace Aspire.Hosting;

public static partial class BouncyHsmBuilderInitializeExtensions
{
    [AspireExport]
    public static IResourceBuilder<BouncyHsmResource> WithInitializedAction(this IResourceBuilder<BouncyHsmResource> builder, BouncyHsmInitializeTask initializedAction)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(initializedAction);

        builder.Resource.InitializeTasks.Add(initializedAction);

        return builder;
    }
}