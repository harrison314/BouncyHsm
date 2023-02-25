using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal record struct MechanismInfo(uint MinKeySize, uint MaxKeySize, MechanismCkf Flags, MechanismCkf RequireParamsIn);
