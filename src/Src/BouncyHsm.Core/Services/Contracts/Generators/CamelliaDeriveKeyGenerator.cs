using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class CamelliaDeriveKeyGenerator : BufferedCipherDeriveKeyGenerator<CamelliaKeyObject>
{
    public CamelliaDeriveKeyGenerator(IBufferedCipher bufferedCipher, CKM mechanism, byte[] data, byte[]? iv, ILogger<CamelliaDeriveKeyGenerator> logger)
        : base(bufferedCipher, mechanism, data, iv, logger)
    {

    }

    public override string ToString()
    {
        return $"CamelliaDeriveKeyGenerator with {this.bufferedCipher.AlgorithmName}";
    }
}