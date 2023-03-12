
namespace BouncyHsm.Services.Configuration;

public class BouncyHsmSetup
{
    public TcpEndpointSetup? TcpEndpoint
    {
        get;
        set;
    }

    public bool EnableSwagger
    {
        get;
        set;
    }

    public BouncyHsmSetup()
    {

    }
}
