namespace BouncyHsm.Core.UseCases.Contracts;

public class MigrationRequest
{
    public bool ResetAllowedMechanism
    {
        get;
        set;
    }

    public MigrationRequest()
    {
        
    }
}