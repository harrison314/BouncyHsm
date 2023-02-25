using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.Contracts;

internal interface IDeriveKeyGenerator
{
    void Init(IReadOnlyDictionary<CKA, IAttributeValue> template);

    SecretKeyObject Generate(StorageObject baseKey);
}