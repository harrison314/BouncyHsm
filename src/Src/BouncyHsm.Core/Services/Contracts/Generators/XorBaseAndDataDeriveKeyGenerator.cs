using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class XorBaseAndDataDeriveKeyGenerator : DeriveKeyGeneratorBase
{
    private readonly byte[] data;

    public XorBaseAndDataDeriveKeyGenerator(byte[] data, ILogger<XorBaseAndDataDeriveKeyGenerator> logger)
        : base(logger)
    {
        this.data = data;
    }

    public override string ToString()
    {
        return $"XorBaseAndDataDeriveKeyGenerator with data {Convert.ToHexString(this.data)}";
    }

    protected override byte[] DeriveSecret(SecretKeyObject generatedKey, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        byte[] baseKeySecet = baseKey.GetSecret();
        int newSecretLen;

        if (!template.ContainsKey(CKA.CKA_KEY_TYPE) && !template.ContainsKey(CKA.CKA_VALUE_LEN))
        {
            newSecretLen = Math.Min(baseKeySecet.Length, this.data.Length);
            this.logger.LogTrace("Set secred length {SecretLength} as value of minimum from base material length.", newSecretLen);
        }
        else if (template.TryGetValue(CKA.CKA_VALUE_LEN, out IAttributeValue? valzeLen))
        {
            newSecretLen = (int)valzeLen.AsUint();
            this.logger.LogTrace("Set secred length {SecretLength} as value of CKA_VALUE_LEN", newSecretLen);
        }
        else
        {
            newSecretLen = (int)generatedKey.GetMinimalSecretLen();
            this.logger.LogTrace("Set secred length {SecretLength} as minimal length of genrated key.", newSecretLen);
        }

        byte[] newSecret = new byte[newSecretLen];
        byte[] localData = this.data;

        for (int i = 0; i < newSecret.Length; i++)
        {
            newSecret[i] = (byte)(baseKeySecet[i % baseKeySecet.Length] ^ localData[i % localData.Length]);
        }

        return newSecret;
    }

    protected override void UpdatePropertiesAfterCeate(SecretKeyObject generalSecretKeyObject, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        base.UpdatePropertiesAfterCeate(generalSecretKeyObject, baseKey, template);
        generalSecretKeyObject.CkaKeyGenMechanism = CKM.CKM_XOR_BASE_AND_DATA;
    }
}