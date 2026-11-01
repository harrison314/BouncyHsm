using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Security;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class GenericSecretHmacKeyGenerator : IKeyGenerator
{
    private readonly CKM generatingMechanism;
    private readonly CKK keyType;
    private readonly ILogger<GenericSecretHmacKeyGenerator> logger;
    private IReadOnlyDictionary<CKA, IAttributeValue>? template;

    public GenericSecretHmacKeyGenerator(CKM generatingMechanism, CKK keyType, ILogger<GenericSecretHmacKeyGenerator> logger)
    {
        this.generatingMechanism = generatingMechanism;
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
        generalSecretKeyObject.CkaKeyGenMechanism = this.generatingMechanism;
        generalSecretKeyObject.CkaKeyType = this.keyType;

        foreach (KeyValuePair<CKA, IAttributeValue> kvp in this.template)
        {
            if (kvp.Key == CKA.CKA_CLASS)
            {
                continue;
            }

            generalSecretKeyObject.SetValue(kvp.Key, kvp.Value, false);
        }

        this.CheckLength(generalSecretKeyObject);
        
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

    private void CheckLength(GenericSecretKeyObject generalSecretKeyObject)
    {
        bool mechanismFound = MechanismUtils.TryGetMechanismInfo(this.generatingMechanism, out P11Handlers.Common.MechanismInfo info);
        System.Diagnostics.Debug.Assert(mechanismFound);

        uint keySizeInBits = generalSecretKeyObject.CkaValueLen * 8;
        if (info.MinKeySize > keySizeInBits || info.MaxKeySize < keySizeInBits)
        {
            this.logger.LogError("Invalid key size for {Mechanism} actual key size {KeySizeInBits} bits (The permitted range is between {MinKeySize} and {MaxKeySize} bits).",
                this.generatingMechanism,
                keySizeInBits,
                info.MinKeySize,
                info.MaxKeySize);

            throw new RpcPkcs11Exception(CKR.CKR_KEY_SIZE_RANGE,
                $"Invalid key size for {this.generatingMechanism} actual key size {keySizeInBits} bits (The permitted range is between {info.MinKeySize} and {info.MaxKeySize} bits).");
        }
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
              $"Attribute {CKA.CKA_CLASS} must be {CKO.CKO_SECRET_KEY}.");
        }

        if ((CKK)template.GetAttributeUint(CKA.CKA_KEY_TYPE, (uint)this.keyType) != this.keyType)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_KEY_TYPE} must be {this.keyType}.");
        }
    }
}
