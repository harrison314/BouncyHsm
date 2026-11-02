using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Client;

namespace Aspire.Hosting;

public class BouncyHsmResource : ContainerResource, IResourceWithEndpoints
{
   
    internal List<Func<BouncyHsmClient, CancellationToken, Task>> InitializeTasks
    {
        get;
    }

    public BouncyHsmResource(string name, string? entrypoint = null)
        : base(name, entrypoint)
    {
        this.InitializeTasks = new List<Func<BouncyHsmClient, CancellationToken, Task>>();
    }
}
