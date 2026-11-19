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

internal class DigestXofDeriveKeyGenerator : DeriveKeyGeneratorBase
{
    private readonly IXof digest;
    private readonly CKM mchanism;

    public DigestXofDeriveKeyGenerator(IXof digest, CKM mchanism, ILogger<DigestDeriveKeyGenerator> logger)
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

        int valueLen = this.GetValueLen(generatedKey, template);
        byte[] digestValue = new byte[valueLen];
        this.digest.OutputFinal(digestValue.AsSpan());

        return digestValue;
    }

    protected override void UpdatePropertiesAfterCeate(SecretKeyObject generalSecretKeyObject, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        base.UpdatePropertiesAfterCeate(generalSecretKeyObject, baseKey, template);
        generalSecretKeyObject.CkaKeyGenMechanism = this.mchanism;
    }

    public override string ToString()
    {
        return $"DigestXofDeriveKeyGenerator with algorithm: {this.digest.AlgorithmName}";
    }

    private int GetValueLen(SecretKeyObject generalSecretKeyObject, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to GetValueLen.");

        uint? requiredValueLen = generalSecretKeyObject.GetRequiredSecretLen();
        if (requiredValueLen.HasValue)
        {
            return (int)requiredValueLen.Value;
        }

        if (template.ContainsKey(CKA.CKA_VALUE_LEN))
        {
            return (int)template.GetRequiredAttributeUint(CKA.CKA_VALUE_LEN);
        }

        throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCOMPLETE,
            $"Attribute {CKA.CKA_VALUE_LEN} is reqired for this key type in C_DeriveKey.");
    }
}