namespace BouncyHsm.Core.Services.Contracts;

public interface ITimeAccessor
{
    DateTime UtcNow
    {
        get;
    }
}
