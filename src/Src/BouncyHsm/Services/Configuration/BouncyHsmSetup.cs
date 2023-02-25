
namespace BouncyHsm.Services.Configuration;

public class BouncyHsmSetup
{
    public TcpEnspointSetup? TcpEnspoint
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
