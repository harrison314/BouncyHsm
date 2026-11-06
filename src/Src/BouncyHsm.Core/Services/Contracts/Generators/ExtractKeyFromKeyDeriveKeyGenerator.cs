using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
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

        if (this.extractParams > baseSeecret.Length*8)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Can not extract {this.extractParams} bit from key with start bit {baseSeecret.Length}.");
        }

        return this.Extract(baseSeecret, this.extractParams);
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

    private byte[] Extract(byte[] value, int startBit)
    {
        int totalBits = value.Length * 8 - startBit;
        int resultLength = (totalBits + 7) / 8;
        byte[] result = new byte[resultLength];

        for (int i = 0; i < totalBits; i++)
        {
            int sourceBit = startBit + i;
            int sourceByte = sourceBit / 8;
            int sourceOffset = sourceBit % 8;

            int bit = (value[sourceByte] >> sourceOffset) & 1;

            int targetByte = i / 8;
            int targetOffset = i % 8;
            if (bit == 1)
            {
                result[targetByte] |= (byte)(1 << targetOffset);
            }
        }

        return result;
    }
}