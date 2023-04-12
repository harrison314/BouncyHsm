using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class AesDeriveKeyGenerator : IDeriveKeyGenerator
{
    private readonly IBufferedCipher bufferedCipher;
    private readonly byte[] data;
    private readonly byte[]? iv;
    private readonly ILogger<AesDeriveKeyGenerator> logger;
    private IReadOnlyDictionary<CKA, IAttributeValue>? template;

    public AesDeriveKeyGenerator(IBufferedCipher bufferedCipher, byte[] data, byte[]? iv, ILogger<AesDeriveKeyGenerator> logger)
    {
        this.bufferedCipher = bufferedCipher;
        this.data = data;
        this.iv = iv;
        this.logger = logger;
        this.template = null;
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


        ICipherParameters cipherParams = this.CreateCipherParams(sBaseKey);

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


        byte[] secret = this.CreateSecret(cipherParams);

        this.logger.LogInformation("Derived secret with length {secretLen}.", secret.Length);

        generalSecretKeyObject.SetSecret(secret);

        if (this.template.ContainsKey(CKA.CKA_VALUE_LEN))
        {
            uint requestdValueLen = this.template.GetRequiredAttributeUint(CKA.CKA_VALUE_LEN);
            this.TryUpdateValueLen(generalSecretKeyObject, (int)requestdValueLen);
        }

        generalSecretKeyObject.CkaAlwaysSensitive = sBaseKey.CkaAlwaysSensitive;
        generalSecretKeyObject.CkaNewerExtractable = sBaseKey.CkaNewerExtractable;
        generalSecretKeyObject.CkaLocal = false; //TODO: Check withspecification

        return generalSecretKeyObject;
    }

    private byte[] CreateSecret(ICipherParameters cipherParams)
    {
        this.logger.LogTrace("Entering to CreateSecret");

        this.bufferedCipher.Reset();
        this.bufferedCipher.Init(true, cipherParams);

        return this.bufferedCipher.DoFinal(this.data);
    }

    public override string ToString()
    {
        return $"AesDeriveKeyGenerator with {this.bufferedCipher.AlgorithmName}";
    }

    private ICipherParameters CreateCipherParams(KeyObject keyObject)
    {
        this.logger.LogTrace("Entering to CreateCipherParams.");

        if (keyObject is AesKeyObject aesKeyObject)
        {
            if (!aesKeyObject.CkaDerive)
            {
                this.logger.LogError("Object with id {ObjectId} can not set CKA_DERVIVE to true.", keyObject.Id);
                throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED,
                    "The derive operation is not allowed because objet is not authorized to derive key (CKA_DERVIVE must by true).");
            }

            if (this.iv == null)
            {
                return new KeyParameter(aesKeyObject.GetSecret());
            }
            else
            {
                return new ParametersWithIV(new KeyParameter(aesKeyObject.GetSecret()), this.iv);
            }
        }
        else
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_HANDLE_INVALID, $"This mechanism required AES key.");
        }
    }

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
