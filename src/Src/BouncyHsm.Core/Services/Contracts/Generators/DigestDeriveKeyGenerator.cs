using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class DigestDeriveKeyGenerator : DeriveKeyGeneratorBase
{
    private readonly IDigest digest;
    private readonly CKM mchanism;

    public DigestDeriveKeyGenerator(IDigest digest, CKM mchanism, ILogger<DigestDeriveKeyGenerator> logger)
        : base(logger)
    {
        this.digest = digest;
        this.mchanism = mchanism;
    }

    protected override byte[] DeriveSecret(SecretKeyObject generatedKey, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to DeriveSecret with digest {digest}.", this.digest.AlgorithmName);

        this.digest.Reset();
        this.digest.BlockUpdate(baseKey.GetSecret());

        byte[] digestValue = new byte[this.digest.GetDigestSize()];
        this.digest.DoFinal(digestValue);

        return digestValue;
    }

    protected override void UpdatePropertiesAfterCeate(SecretKeyObject generalSecretKeyObject, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        base.UpdatePropertiesAfterCeate(generalSecretKeyObject, baseKey, template);
        generalSecretKeyObject.CkaKeyGenMechanism = this.mchanism;
    }

    public override string ToString()
    {
        return $"DigestDeriveKeyGenerator with algorithm: {this.digest.AlgorithmName}";
    }
}
