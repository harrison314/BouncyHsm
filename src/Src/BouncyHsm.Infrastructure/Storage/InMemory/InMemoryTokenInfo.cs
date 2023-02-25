using BouncyHsm.Core.Services.Contracts.Entities;

namespace BouncyHsm.Infrastructure.Storage.InMemory;

public class InMemoryTokenInfo : TokenInfo
{
    public string UserPin
    {
        get;
        set;
    }

    public string SoPin
    {
        get;
        set;
    }

    public string? SignaturePin
    {
        get;
        set;
    }

    public InMemoryTokenInfo()
    {
        this.UserPin = string.Empty;
        this.SoPin = string.Empty;
    }
}