namespace BouncyHsm.Core.Services.Contracts;

public sealed class EmptySessionState : ISessionState
{
    public static readonly EmptySessionState Instance = new EmptySessionState();

    private EmptySessionState()
    {

    }

    public override string ToString()
    {
        return "Empty";
    }
}
