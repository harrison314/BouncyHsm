using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class ConcatDataAndBaseDeriveKeyGenerator : DeriveKeyGeneratorBase
{
    private readonly byte[] data;

    public ConcatDataAndBaseDeriveKeyGenerator(byte[] data, ILogger<ConcatDataAndBaseDeriveKeyGenerator> logger)
        : base(logger)
    {
        this.data = data;
    }

    protected override byte[] DeriveSecret(SecretKeyObject generatedKey, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to DeriveSecret with base key {baseKeyId}.", baseKey.Id);

        byte[] baseKeySecet = baseKey.GetSecret();

        byte[] concatedSecret = new byte[this.data.Length + baseKeySecet.Length];
        System.Buffer.BlockCopy(this.data, 0, concatedSecret, 0, this.data.Length);
        System.Buffer.BlockCopy(baseKeySecet, 0, concatedSecret, this.data.Length, baseKeySecet.Length);

        return concatedSecret;
    }

    public override string ToString()
    {
        return "ConcatDataAndBase";
    }
}
