using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class ConcatBaseAndKeyDeriveKeyGenerator : DeriveKeyGeneratorBase
{
    private readonly SecretKeyObject otherKey;

    public ConcatBaseAndKeyDeriveKeyGenerator(SecretKeyObject otherKey, ILogger<ConcatBaseAndKeyDeriveKeyGenerator> logger)
        : base(logger)
    {
        this.otherKey = otherKey;
    }

    protected override byte[] DeriveSecret(SecretKeyObject generatedKey, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to DeriveSecret with base key {baseKeyId}.", baseKey.Id);

        byte[] baseKeySecet = baseKey.GetSecret();
        byte[] data = this.otherKey.GetSecret();

        byte[] concatedSecret = new byte[baseKeySecet.Length + data.Length];
        System.Buffer.BlockCopy(baseKeySecet, 0, concatedSecret, 0, baseKeySecet.Length);
        System.Buffer.BlockCopy(data, 0, concatedSecret, baseKeySecet.Length, data.Length);

        return concatedSecret;
    }

    protected override void UpdatePropertiesAfterCeate(SecretKeyObject generalSecretKeyObject, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        generalSecretKeyObject.CkaSensitive = baseKey.CkaSensitive && this.otherKey.CkaSensitive;
        if (!generalSecretKeyObject.CkaSensitive && template.TryGetValue(CKA.CKA_SENSITIVE, out IAttributeValue? attrValue))
        {
            generalSecretKeyObject.SetValue(CKA.CKA_SENSITIVE, attrValue);
        }

        generalSecretKeyObject.CkaExtractable = baseKey.CkaExtractable && this.otherKey.CkaExtractable;
        if (!generalSecretKeyObject.CkaExtractable && template.TryGetValue(CKA.CKA_EXTRACTABLE, out IAttributeValue? extractableAttrValue))
        {
            generalSecretKeyObject.SetValue(CKA.CKA_EXTRACTABLE, extractableAttrValue);
        }

        generalSecretKeyObject.CkaNewerExtractable = baseKey.CkaNewerExtractable && this.otherKey.CkaNewerExtractable;
    }

    public override string ToString()
    {
        return "ConcatBaseAndKey";
    }
}
