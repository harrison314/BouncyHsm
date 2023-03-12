
namespace BouncyHsm.Services.Configuration;

public class TcpEndpointSetup
{
    public string Endpoint
    {
        get;
        set;
    }

    public TimeSpan? ReceiveTimeout
    {
        get;
        set;
    }

    public TimeSpan? SendTimeout
    {
        get;
        set;
    }

    public TcpEndpointSetup()
    {
        this.Endpoint = string.Empty;
    }
}