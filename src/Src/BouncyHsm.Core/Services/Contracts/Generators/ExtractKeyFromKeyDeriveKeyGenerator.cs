using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;

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

        if (this.extractParams == 0)
        {
            return Array.Empty<byte>();
        }

        int extratBytes = Math.DivRem(this.extractParams, 8, out int extractAnotherBits);
        int newArraySize = extratBytes + (extractAnotherBits > 0 ? 1 : 0);

        byte[] baseSeecret = baseKey.GetSecret();
        if (newArraySize > baseSeecret.Length)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Can not extract {this.extractParams} bit from key with lenght {baseSeecret.Length} bytes.");
        }

        byte[] newSceecrit = new byte[newArraySize];
        System.Buffer.BlockCopy(baseSeecret, 0, newSceecrit, 0, newSceecrit.Length);

        if (extractAnotherBits > 0)
        {
            int lastIndex = newArraySize - 1;
            newSceecrit[lastIndex] = (byte)(newSceecrit[lastIndex] & this.CalculateMask(extractAnotherBits));
        }

        return newSceecrit;
    }

    private byte CalculateMask(int extractAnotherBits)
    {
        System.Diagnostics.Debug.Assert(extractAnotherBits < 8);

        uint mask = 0b00000000;
        for (int i = 0; i < extractAnotherBits; i++)
        {
            mask = (mask >> 1) | 0b10000000;
        }

        return (byte)(mask & 0xFF);
    }

    public override string ToString()
    {
        return $"ExtractKeyFromKey with {this.extractParams} bits";
    }
}