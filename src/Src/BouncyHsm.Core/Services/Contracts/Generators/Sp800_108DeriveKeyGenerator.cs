using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal abstract class Sp800_108DeriveKeyGenerator : IDeriveKeyGenerator
{
    protected readonly ILogger logger;
    protected IReadOnlyDictionary<CKA, IAttributeValue>? template;

    protected CKM KdfMechanism
    {
        get;
    }

    protected IPrfDataParam[] DataParams
    {
        get;
    }

    protected Sp800_108DeriveKeyGenerator(CKM kdfMechanism, IPrfDataParam[] dataParams, ILogger logger)
    {
        this.KdfMechanism = kdfMechanism;
        this.DataParams = dataParams;
        this.logger = logger;
        this.template = null;
    }

    public void Init(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to Init");

        this.CheckTemplate(template);
        this.template = template;

        this.CheckDataParams(this.DataParams);
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
            if (kvp.Key == CKA.CKA_CLASS || kvp.Key == CKA.CKA_KEY_TYPE)
            {
                continue;
            }

            generalSecretKeyObject.SetValue(kvp.Key, kvp.Value, false);
        }

        int requestedValueLen = this.GetValueLen(generalSecretKeyObject);

        byte[] secret = this.DriveKey(sBaseKey.GetSecret(), requestedValueLen);
        this.logger.LogInformation("Derived secret with length {secretLen}.", secret.Length);

        generalSecretKeyObject.SetSecret(secret);

        generalSecretKeyObject.CkaAlwaysSensitive = sBaseKey.CkaAlwaysSensitive;
        generalSecretKeyObject.CkaNewerExtractable = sBaseKey.CkaNewerExtractable;
        generalSecretKeyObject.CkaLocal = false;

        return generalSecretKeyObject;
    }

    protected abstract byte[] DriveKey(byte[] keyValue, int requestedValueLen);

    protected abstract void CheckDataParams(IPrfDataParam[] dataParams);

    private int GetValueLen(SecretKeyObject generalSecretKeyObject)
    {
        this.logger.LogTrace("Entering to GetValueLen.");

        System.Diagnostics.Debug.Assert(this.template != null);

        uint? requiredValueLen = generalSecretKeyObject.GetRequiredSecretLen();
        if (requiredValueLen.HasValue)
        {
            return (int)requiredValueLen.Value;
        }

        if (this.template.ContainsKey(CKA.CKA_VALUE_LEN))
        {
            return (int)this.template.GetRequiredAttributeUint(CKA.CKA_VALUE_LEN);
        }

        throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
            $"Attribute {CKA.CKA_VALUE_LEN} is reqired for this key type in C_DeriveKey.");
    }

    protected virtual void CheckTemplate(IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        if (template.ContainsKey(CKA.CKA_VALUE))
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE} can not use in C_DeriveKey.");
        }

        if (template.GetAttributeUint(CKA.CKA_VALUE_LEN, 1) < 1)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_VALUE_LEN} is less than 1.");
        }

        if ((CKO)template.GetAttributeUint(CKA.CKA_CLASS, (uint)CKO.CKO_SECRET_KEY) != CKO.CKO_SECRET_KEY)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT,
              $"Attribute {CKA.CKA_CLASS} must be {CKO.CKO_SECRET_KEY}.");
        }
    }
}
