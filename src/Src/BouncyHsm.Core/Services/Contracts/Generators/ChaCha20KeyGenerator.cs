using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class ChaCha20KeyGenerator : IKeyGenerator
{
    private readonly ILogger<ChaCha20KeyGenerator> logger;
    private IReadOnlyDictionary<CKA, IAttributeValue>? template;

    public ChaCha20KeyGenerator(ILogger<ChaCha20KeyGenerator> logger)
    {
        this.logger = logger;
    }

    public void Init(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to init");

        this.CheckTemplate(template);

        this.template = template;
    }

    public SecretKeyObject Generate(SecureRandom secureRandom)
    {
        this.logger.LogTrace("Entering to Generate");

        System.Diagnostics.Debug.Assert(this.template != null);

        ChaCha20KeyObject keyObject = new ChaCha20KeyObject();

        foreach (KeyValuePair<CKA, IAttributeValue> kvp in this.template)
        {
            if (kvp.Key == CKA.CKA_CLASS || kvp.Key == CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            keyObject.SetValue(kvp.Key, kvp.Value, false);
        }

        byte[] value = new byte[32];
        secureRandom.NextBytes(value);

        keyObject.SetSecret(value);

        this.logger.LogInformation("Created new ChaCha20 key {keyType} wit lenght {keyLength}.",
            keyObject.CkaKeyType,
            keyObject.CkaValueLen);

        return keyObject;
    }

    public override string ToString()
    {
        return "ChaCha20KeyGenerator";
    }

    private void CheckTemplate(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        if (template.ContainsKey(CKA.CKA_VALUE))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE} can not use in C_GenerateKey.");
        }

        if (template.GetAttributeUint(CKA.CKA_VALUE_LEN, 32) != 32)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE_LEN} is invalid for POLY1305 key.");
        }

        if ((CKO)template.GetAttributeUint(CKA.CKA_CLASS, (uint)CKO.CKO_SECRET_KEY) != CKO.CKO_SECRET_KEY)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_CLASS} must by {CKO.CKO_SECRET_KEY}.");
        }

        if ((CKK)template.GetAttributeUint(CKA.CKA_KEY_TYPE, (uint)CKK.CKK_CHACHA20) != CKK.CKK_CHACHA20)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_KEY_TYPE} must by {CKK.CKK_CHACHA20}.");
        }
    }
}