using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class AesKeyGenerator : IKeyGenerator
{
    private readonly ILogger<AesKeyGenerator> logger;
    private IReadOnlyDictionary<CKA, IAttributeValue>? template;

    public AesKeyGenerator(ILogger<AesKeyGenerator> logger)
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

        AesKeyObject generalSecretKeyObject = new AesKeyObject();

        foreach (KeyValuePair<CKA, IAttributeValue> kvp in this.template)
        {
            if (kvp.Key == CKA.CKA_CLASS || kvp.Key == CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            generalSecretKeyObject.SetValue(kvp.Key, kvp.Value, false);
        }

        int valueLen = (int)generalSecretKeyObject.CkaValueLen;

        CipherKeyGenerator keyGenerator = new CipherKeyGenerator();
        keyGenerator.Init(new KeyGenerationParameters(secureRandom, valueLen * 8));
        byte[] value = keyGenerator.GenerateKey();

        generalSecretKeyObject.SetSecret(value);

        this.logger.LogInformation("Created new AES-{keySize} key {keyType} wit lenght {keyLength}.",
            valueLen * 8,
            generalSecretKeyObject.CkaKeyType,
            generalSecretKeyObject.CkaValueLen);

        return generalSecretKeyObject;
    }

    public override string ToString()
    {
        return "AesKeyGenerator";
    }

    private void CheckTemplate(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        if (template.ContainsKey(CKA.CKA_VALUE))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE} can not use in C_GenerateKey.");
        }

        uint valueLength = template.GetRequiredAttributeUint(CKA.CKA_VALUE_LEN);
        if (!AesKeyObject.IsKeySizeValid((int)valueLength))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE_LEN} is invalid for AES key.");
        }

        if ((CKO)template.GetAttributeUint(CKA.CKA_CLASS, (uint)CKO.CKO_SECRET_KEY) != CKO.CKO_SECRET_KEY)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_CLASS} must by {CKO.CKO_SECRET_KEY}.");
        }

        if ((CKK)template.GetAttributeUint(CKA.CKA_KEY_TYPE, (uint)CKK.CKK_AES) != CKK.CKK_AES)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_KEY_TYPE} must by {CKK.CKK_AES}.");
        }
    }
}