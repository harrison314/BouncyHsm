using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Generators;

internal class HkdfDeriveKeyGenerator : DeriveKeyGeneratorBase
{
    public const uint CKF_HKDF_SALT_NULL = 0x00000001;
    public const uint CKF_HKDF_SALT_DATA = 0x00000002;
    public const uint CKF_HKDF_SALT_KEY = 0x00000004;

    private readonly Ckp_CkHkdfParams hkdfParams;
    private readonly SecretKeyObject? saltKeyObject;
    private readonly IDigest digetAlgorithm;

    public HkdfDeriveKeyGenerator(Ckp_CkHkdfParams hkdfParams,
        SecretKeyObject? saltKeyObject,
        IDigest digetAlgorithm,
        ILogger<HkdfDeriveKeyGenerator> logger)
        : base(logger)
    {
        this.hkdfParams = hkdfParams;
        this.saltKeyObject = saltKeyObject;
        this.digetAlgorithm = digetAlgorithm;
    }

    protected override byte[] DeriveSecret(SecretKeyObject generatedKey, SecretKeyObject baseKey, IReadOnlyDictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to DeriveSecret HKDF with digest {digest}.", this.digetAlgorithm.AlgorithmName);

        if (!this.hkdfParams.Expand && !this.hkdfParams.Extract)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                $"Parameters pExpand or pExtract or both must by set to true in CK_HKDF_PARAMS.");
        }

        HkdfParameters bcParams;
        byte[]? info = this.hkdfParams.Expand ? this.hkdfParams.Info : null;
        byte[]? salt = this.CreateSalt();

        if (this.hkdfParams.Extract)
        {
            bcParams = new HkdfParameters(baseKey.GetSecret(), salt, info);
        }
        else
        {
            bcParams = HkdfParameters.SkipExtractParameters(baseKey.GetSecret(), info);
        }

        HkdfBytesGenerator hkdfBytesGenerator = new HkdfBytesGenerator(this.digetAlgorithm);
        hkdfBytesGenerator.Init(bcParams);

        uint minKeySize = template.GetAttributeUint(CKA.CKA_VALUE_LEN, generatedKey.GetMinimalSecretLen());
        byte[] bytes = new byte[minKeySize];
        hkdfBytesGenerator.GenerateBytes(bytes, 0, bytes.Length);

        return bytes;
    }

    private byte[]? CreateSalt()
    {
        if (this.hkdfParams.SaltType == CKF_HKDF_SALT_NULL)
        {
            return null;
        }

        if (this.hkdfParams.SaltType == CKF_HKDF_SALT_KEY)
        {
            if (this.saltKeyObject == null)
            {
                this.logger.LogError("In CK_HKDF_PARAMS is iset ulSaltType to CKF_HKDF_SALT_KEY and object is not present.");
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    $"In CK_HKDF_PARAMS is iset ulSaltType to CKF_HKDF_SALT_KEY and object is not present.");
            }

            return this.saltKeyObject.GetSecret();
        }

        if (this.hkdfParams.SaltType == CKF_HKDF_SALT_DATA)
        {
            if (this.hkdfParams.Salt == null)
            {
                this.logger.LogError("In CK_HKDF_PARAMS is iset ulSaltType to CKF_HKDF_SALT_KEY and pSalt is NULL.");
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    $"In CK_HKDF_PARAMS is iset ulSaltType to CKF_HKDF_SALT_KEY and pSalt is NULL.");
            }

            return this.hkdfParams.Salt;
        }

        this.logger.LogError("Invalid value of ulSaltType in CK_HKDF_PARAMS.");
        throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    $"Invalid value of ulSaltType in CK_HKDF_PARAMS.");
    }
}
