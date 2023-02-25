using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.UseCases.Contracts;

public record MechanismInfoData(CKM MechanismType, uint MinKeySize, uint MaxKeySize, MechanismCkf Flags);