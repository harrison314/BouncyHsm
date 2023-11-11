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
        byte[] newSecret = new byte[baseKeySecet.Length];
        byte[] localData = this.data;

        for (int i = 0; i < baseKeySecet.Length; i++)
        {
            newSecret[i] = (byte)(baseKeySecet[i] ^ localData[i % localData.Length]);
        }

        return newSecret;
    }
}