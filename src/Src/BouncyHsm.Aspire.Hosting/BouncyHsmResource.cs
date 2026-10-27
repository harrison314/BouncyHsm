using Aspire.Hosting.ApplicationModel;

namespace Aspire.Hosting;

public class BouncyHsmResource : ContainerResource, IResourceWithEndpoints
{
   
    public BouncyHsmResource(string name, string? entrypoint = null)
        : base(name, entrypoint)
    {
    }
}
