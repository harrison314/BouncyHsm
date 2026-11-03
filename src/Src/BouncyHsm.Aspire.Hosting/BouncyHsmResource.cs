using Aspire.Hosting.ApplicationModel;
using BouncyHsm.Client;
using Microsoft.Extensions.Logging;

namespace Aspire.Hosting;

public class BouncyHsmResource : ContainerResource, IResourceWithEndpoints
{
   
    internal List<BouncyHsmInitializeTask> InitializeTasks
    {
        get;
    }

    public BouncyHsmResource(string name, string? entrypoint = null)
        : base(name, entrypoint)
    {
        this.InitializeTasks = new List<BouncyHsmInitializeTask>();
    }
}

