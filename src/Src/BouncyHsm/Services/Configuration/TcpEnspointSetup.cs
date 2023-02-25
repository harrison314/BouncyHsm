
namespace BouncyHsm.Services.Configuration;

public class TcpEnspointSetup
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

    public TcpEnspointSetup()
    {
        this.Endpoint = string.Empty;
    }
}