using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.UseCases.Contracts;

public interface IKeysGenerationFacade
{
    Task<DomainResult<GeneratedKeyPairIds>> GenerateRsaKeyPair(uint slotId, GenerateRsaKeyPairRequest request, CancellationToken cancellationToken);

    Task<DomainResult<GeneratedKeyPairIds>> GenerateEcKeyPair(uint slotId, GenerateEcKeyPairRequest request, CancellationToken cancellationToken);

    Task<DomainResult<GeneratedSecretId>> GenerateSecretKey(uint slotId, GenerateSecretKeyRequest request, CancellationToken cancellationToken);

    Task<DomainResult<GeneratedSecretId>> GenerateAesKey(uint slotId, GenerateAesKeyRequest request, CancellationToken cancellationToken);
}
