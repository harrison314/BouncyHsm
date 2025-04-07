using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

public record struct MechanismInfo(uint MinKeySize, uint MaxKeySize, MechanismCkf Flags, MechanismCkf RequireParamsIn, Pkcs11SpecVersion SpecVersion);

public enum Pkcs11SpecVersion
{
    V2_40
}
