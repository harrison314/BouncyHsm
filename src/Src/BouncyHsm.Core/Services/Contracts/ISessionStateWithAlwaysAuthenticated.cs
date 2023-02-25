namespace BouncyHsm.Core.Services.Contracts;

public interface ISessionStateWithAlwaysAuthenticated : ISessionState
{
    public bool RequireContextPin
    {
        get;
    }

    public bool IsContextPinHasSet
    {
        get;
    }

    void ContextLogin();
}