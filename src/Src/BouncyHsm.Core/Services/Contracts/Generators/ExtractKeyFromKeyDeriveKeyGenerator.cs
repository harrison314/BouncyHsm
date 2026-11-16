using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Utils;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class ExtractKeyFromKeyDeriveKeyGenerator : DeriveKeyGeneratorBase
{
    private readonly int extractParams;

    public ExtractKeyFromKeyDeriveKeyGenerator(int extractParams, ILogger<ExtractKeyFromKeyDeriveKeyGenerator> logger)
        : base(logger)
    {
        this.extractParams = extractParams;
    }

    protected override byte[] DeriveSecret(SecretKeyObject generatedKey, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to DeriveSecret with base key {baseKeyId}.", baseKey.Id);

        byte[] baseSeecret = baseKey.GetSecret();
        if (this.extractParams == 0)
        {
            return baseSeecret;
        }

        if (this.extractParams > baseSeecret.Length * 8)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Can not extract {this.extractParams} bit from key with start bit {baseSeecret.Length}.");
        }

        int length;
        if (template.TryGetValue(CKA.CKA_VALUE_LEN, out IAttributeValue? ckaValueLen))
        {
            length = Convert.ToInt32(ckaValueLen.AsUint());
        }
        else
        {
            int totalBits = baseSeecret.Length * 8 - this.extractParams;
            length = (totalBits + 7) / 8;
        }

        return this.Extract(baseSeecret, this.extractParams, length);
    }

    protected override void UpdatePropertiesAfterCeate(SecretKeyObject generalSecretKeyObject, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        base.UpdatePropertiesAfterCeate(generalSecretKeyObject, baseKey, template);
        generalSecretKeyObject.CkaKeyGenMechanism = CKM.CKM_EXTRACT_KEY_FROM_KEY;
    }

    public override string ToString()
    {
        return $"ExtractKeyFromKey with {this.extractParams} bits";
    }

    public byte[] Extract(byte[] value, int startBit, int length)
    {
        byte[] result = new byte[length];
        BitSpan destination = new BitSpan(result);
        BitSpan src = new BitSpan(value);

        for (int i = 0; i < length * 8; i++)
        {
            destination[i] = src[(startBit + i) % src.BitLength];
        }

        return result;
    }
}