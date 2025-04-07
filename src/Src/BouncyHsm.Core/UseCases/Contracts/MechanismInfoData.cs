using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;

namespace BouncyHsm.Core.UseCases.Contracts;

public record MechanismInfoData(CKM MechanismType,
    uint MinKeySize,
    uint MaxKeySize,
    MechanismCkf Flags,
    Pkcs11SpecVersion SpecificationVersion);
