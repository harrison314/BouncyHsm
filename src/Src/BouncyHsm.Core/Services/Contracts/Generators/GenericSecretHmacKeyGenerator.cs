using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class GenericSecretHmacKeyGenerator : IKeyGenerator
{
    private readonly CKK keyType;
    private readonly ILogger<GenericSecretHmacKeyGenerator> logger;
    private IReadOnlyDictionary<CKA, IAttributeValue>? template;

    public GenericSecretHmacKeyGenerator(CKK keyType, ILogger<GenericSecretHmacKeyGenerator> logger)
    {
        this.keyType = keyType;
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

        GenericSecretKeyObject generalSecretKeyObject = new GenericSecretKeyObject();
        generalSecretKeyObject.CkaKeyType = this.keyType;

        foreach (KeyValuePair<CKA, IAttributeValue> kvp in this.template)
        {
            if (kvp.Key == CKA.CKA_CLASS)
            {
                continue;
            }

            generalSecretKeyObject.SetValue(kvp.Key, kvp.Value, false);
        }

        byte[] value = new byte[generalSecretKeyObject.CkaValueLen];
        secureRandom.NextBytes(value);
        generalSecretKeyObject.SetSecret(value);

        this.logger.LogInformation("Created new seecret key {keyType} wit lenght {keyLength}.",
            generalSecretKeyObject.CkaKeyType,
            generalSecretKeyObject.CkaValueLen);

        return generalSecretKeyObject;
    }

    public override string ToString()
    {
        return "GenericSecretHmacKeyGenerator";
    }

    private void CheckTemplate(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        if (template.ContainsKey(CKA.CKA_VALUE))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE} can not use in C_GenerateKey.");
        }

        if (template.GetRequiredAttributeUint(CKA.CKA_VALUE_LEN) < 1)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE_LEN} is less than 1.");
        }

        if ((CKO)template.GetAttributeUint(CKA.CKA_CLASS, (uint)CKO.CKO_SECRET_KEY) != CKO.CKO_SECRET_KEY)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_CLASS} must by {CKO.CKO_SECRET_KEY}.");
        }

        if ((CKK)template.GetAttributeUint(CKA.CKA_KEY_TYPE, (uint)this.keyType) != this.keyType)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_KEY_TYPE} must by {this.keyType}.");
        }
    }
}
