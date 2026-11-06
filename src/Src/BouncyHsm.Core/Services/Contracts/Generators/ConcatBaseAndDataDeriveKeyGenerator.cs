using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class ConcatBaseAndDataDeriveKeyGenerator : DeriveKeyGeneratorBase
{
    private readonly byte[] data;

    public ConcatBaseAndDataDeriveKeyGenerator(byte[] data, ILogger<ConcatBaseAndDataDeriveKeyGenerator> logger)
        : base(logger)
    {
        this.data = data;
    }

    protected override byte[] DeriveSecret(SecretKeyObject generatedKey, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to DeriveSecret with base key {baseKeyId}.", baseKey.Id);

        byte[] baseKeySecet = baseKey.GetSecret();

        byte[] concatedSecret = new byte[baseKeySecet.Length + this.data.Length];
        System.Buffer.BlockCopy(baseKeySecet, 0, concatedSecret, 0, baseKeySecet.Length);
        System.Buffer.BlockCopy(this.data, 0, concatedSecret, baseKeySecet.Length, this.data.Length);

        return concatedSecret;
    }

    protected override void UpdatePropertiesAfterCeate(SecretKeyObject generalSecretKeyObject, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        base.UpdatePropertiesAfterCeate(generalSecretKeyObject, baseKey, template);
        generalSecretKeyObject.CkaKeyGenMechanism = CKM.CKM_CONCATENATE_BASE_AND_DATA;
    }

    public override string ToString()
    {
        return "ConcatBaseAndData";
    }
}
