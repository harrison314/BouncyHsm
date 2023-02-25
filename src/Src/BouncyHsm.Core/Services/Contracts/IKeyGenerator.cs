using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts;

internal interface IKeyGenerator
{
    void Init(IReadOnlyDictionary<CKA, IAttributeValue> template);

    SecretKeyObject Generate(SecureRandom secureRandom);
}
