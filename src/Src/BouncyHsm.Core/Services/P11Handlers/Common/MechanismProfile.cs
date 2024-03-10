using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

public record MechanismProfile(Dictionary<CKM, MechanismInfo> Mechanims, string Name);