using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal abstract class DeriveKeyGeneratorBase : IDeriveKeyGenerator
{
    protected readonly ILogger logger;
    private IReadOnlyDictionary<CKA, IAttributeValue>? template;

    protected DeriveKeyGeneratorBase(ILogger logger)
    {
        this.logger = logger;
    }

    public void Init(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to init");

        this.CheckTemplate(template);

        this.template = template;
    }

    public SecretKeyObject Generate(StorageObject baseKey)
    {
        this.logger.LogTrace("Entering to Generate.");

        System.Diagnostics.Debug.Assert(this.template != null);

        if (baseKey is not SecretKeyObject)
        {
            this.logger.LogError("Base key handle is invalid  is {baseKey}. Excepted SecretKeyObject.", baseKey);
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID,
              $"Base key handle is invalid, is not SecretKeyObject.");
        }

        SecretKeyObject sBaseKey = (SecretKeyObject)baseKey;

        SecretKeyObject generalSecretKeyObject = StorageObjectFactory.CreateSecret(this.template);
        generalSecretKeyObject.CkaSensitive = sBaseKey.CkaSensitive;
        generalSecretKeyObject.CkaExtractable = sBaseKey.CkaExtractable;

        foreach (KeyValuePair<CKA, IAttributeValue> kvp in this.template)
        {
            if (kvp.Key == CKA.CKA_CLASS)
            {
                continue;
            }

            generalSecretKeyObject.SetValue(kvp.Key, kvp.Value, false);
        }

        byte[] secret = this.DeriveSecret(generalSecretKeyObject, sBaseKey, this.template);
        this.logger.LogInformation("Derived secret with length {secretLen}.", secret.Length);

        generalSecretKeyObject.SetSecret(secret);

        if (this.template.ContainsKey(CKA.CKA_VALUE_LEN))
        {
            uint requestdValueLen = this.template.GetRequiredAttributeUint(CKA.CKA_VALUE_LEN);
            this.TryUpdateValueLen(generalSecretKeyObject, (int)requestdValueLen);
        }

        generalSecretKeyObject.CkaAlwaysSensitive = sBaseKey.CkaAlwaysSensitive;
        generalSecretKeyObject.CkaNewerExtractable = sBaseKey.CkaNewerExtractable;
        generalSecretKeyObject.CkaLocal = false;

        this.UpdatePropertiesAfterCeate(generalSecretKeyObject, sBaseKey, this.template);

        return generalSecretKeyObject;
    }

    protected abstract byte[] DeriveSecret(SecretKeyObject generatedKey, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template);

    protected virtual void CheckTemplate(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        if (template.ContainsKey(CKA.CKA_VALUE))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE} can not use in C_GenerateKey.");
        }

        if (template.GetAttributeUint(CKA.CKA_VALUE_LEN, 1) < 1)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE_LEN} is less than 1.");
        }

        if ((CKO)template.GetAttributeUint(CKA.CKA_CLASS, (uint)CKO.CKO_SECRET_KEY) != CKO.CKO_SECRET_KEY)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_CLASS} must by {CKO.CKO_SECRET_KEY}.");
        }
    }

    protected virtual void UpdatePropertiesAfterCeate(SecretKeyObject generalSecretKeyObject, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        //NOP
    }

    private void TryUpdateValueLen(SecretKeyObject generalSecretKeyObject, int requestdValueLen)
    {
        this.logger.LogTrace("Entering to TryUpdateValueLen with requestdValueLen {requestdValueLen}.",
            requestdValueLen);

        byte[] secret = generalSecretKeyObject.GetSecret();
        if (secret.Length < requestdValueLen)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
               $"Attribute {nameof(CKA.CKA_VALUE_LEN)} is inconsistent with used mechanism type. {nameof(CKA.CKA_VALUE_LEN)} has value {requestdValueLen}, maximum is {secret.Length}.");
        }

        if (secret.Length > requestdValueLen)
        {
            generalSecretKeyObject.SetSecret(secret.AsSpan(0, requestdValueLen).ToArray());
            this.logger.LogDebug("Secret is concatet to {requestdValueLen}.", requestdValueLen);
        }
    }
}
