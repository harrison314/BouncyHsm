using BouncyHsm.Core.Services.Contracts.Entities;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class AesDeriveKeyGenerator : BufferedCipherDeriveKeyGenerator<AesKeyObject>
{
    public AesDeriveKeyGenerator(IBufferedCipher bufferedCipher, byte[] data, byte[]? iv, ILogger<AesDeriveKeyGenerator> logger)
        :base(bufferedCipher, data, iv, logger)
    {
       
    }

    public override string ToString()
    {
        return $"AesDeriveKeyGenerator with {this.bufferedCipher.AlgorithmName}";
    }
}
