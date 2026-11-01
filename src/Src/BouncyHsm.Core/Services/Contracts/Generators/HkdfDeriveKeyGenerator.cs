using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Generators;
using Org.BouncyCastle.Crypto.Macs;
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
                $"Parameters pExpand or pExtract or both must be set to true in CK_HKDF_PARAMS.");
        }

        HkdfParameters bcParams;
        byte[]? info = this.hkdfParams.Expand ? this.hkdfParams.Info : null;

        if (this.hkdfParams.Extract)
        {
            byte[] salt = this.CreateSalt();

            if (!this.hkdfParams.Expand)
            {
                return this.Extract(baseKey.GetSecret(), salt);
            }

            bcParams = new HkdfParameters(baseKey.GetSecret(), salt, info);
        }
        else
        {
            bcParams = HkdfParameters.SkipExtractParameters(baseKey.GetSecret(), info);
        }

        HkdfBytesGenerator hkdfBytesGenerator = new HkdfBytesGenerator(this.digetAlgorithm);
        hkdfBytesGenerator.Init(bcParams);

        uint minKeySize = template.GetAttributeUint(CKA.CKA_VALUE_LEN, generatedKey.GetMinimalSecretLen());
        this.CheckLength(minKeySize);
        byte[] bytes = new byte[minKeySize];
        hkdfBytesGenerator.GenerateBytes(bytes, 0, bytes.Length);

        return bytes;
    }

    private void CheckLength(uint minKeySize)
    {
        bool mechanismFound = MechanismUtils.TryGetMechanismInfo(CKM.CKM_HKDF_DERIVE, out P11Handlers.Common.MechanismInfo info);
        System.Diagnostics.Debug.Assert(mechanismFound);

        uint keySizeInBits = minKeySize * 8;
        if (info.MinKeySize > keySizeInBits || info.MaxKeySize < keySizeInBits)
        {
            this.logger.LogError("Invalid key size for {Mechanism} actual key size {KeySizeInBits} bits (The permitted range is between {MinKeySize} and {MaxKeySize} bits).",
                CKM.CKM_HKDF_DERIVE,
                keySizeInBits,
                info.MinKeySize,
                info.MaxKeySize);

            throw new RpcPkcs11Exception(CKR.CKR_KEY_SIZE_RANGE,
                $"Invalid key size for {CKM.CKM_HKDF_DERIVE} actual key size {keySizeInBits} bits (The permitted range is between {info.MinKeySize} and {info.MaxKeySize} bits).");
        }
    }

    private byte[] Extract(byte[] inputKeyMaterial, byte[] salt)
    {
        HMac hmac = new HMac(this.digetAlgorithm);
        hmac.Init(new KeyParameter(salt));
        hmac.BlockUpdate(inputKeyMaterial, 0, inputKeyMaterial.Length);

        byte[] pseudoRandomKey = new byte[hmac.GetMacSize()];
        hmac.DoFinal(pseudoRandomKey, 0);
        return pseudoRandomKey;
    }

    private byte[] CreateSalt()
    {
        if (this.hkdfParams.SaltType == CKF_HKDF_SALT_NULL)
        {
            byte[] zeros = new byte[this.digetAlgorithm.GetDigestSize()];
            Array.Clear(zeros, 0, zeros.Length);
            return zeros;
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
