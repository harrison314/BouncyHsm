namespace BouncyHsm.Core.UseCases.Contracts;

public class OverviewStats
{
    public int ConnectedApplications 
    { 
        get; 
        set;
    }

    public int RoSessionCount
    {
        get;
        set;
    }

    public int RwSessionCount
    {
        get;
        set;
    }

    public int SlotCount
    {
        get;
        set;
    }

    public int TotalObjectCount
    {
        get;
        set;
    }

    public int PrivateKeys
    {
        get;
        set;
    }

    public int X509Certificates
    {
        get;
        set;
    }

    public OverviewStats()
    {

    }
}