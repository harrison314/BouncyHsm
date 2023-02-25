using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts.Entities;

public interface IHardwareFeature : ICryptoApiObject
{
    CKH CkHwFeatureType
    {
        get;
    }
}