﻿using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class WrapperSignerFactory
{
    private readonly ILogger<WrapperSignerFactory> logger;
    private readonly ILoggerFactory loggerFactory;

    public WrapperSignerFactory(ILoggerFactory loggerFactory)
    {
        this.logger = loggerFactory.CreateLogger<WrapperSignerFactory>(); ;
        this.loggerFactory = loggerFactory;
    }

    public IWrapperSigner CreateSignatureAlgorithm(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateSignatureAlgorithm with mechanism type {mechanismType}", mechanism.MechanismType);

        CKM ckMechanism = (CKM)mechanism.MechanismType;

        return ckMechanism switch
        {
            CKM.CKM_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("RSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_SHA1_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("SHA1withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_SHA224_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("SHA224withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_SHA256_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("SHA256withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_SHA384_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("SHA384withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_SHA512_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("SHA512withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_MD2_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("MD2withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_MD5_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("MD5withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_RIPEMD128_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("RIPEMD128withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_RIPEMD160_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("RIPEMD160withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),

            CKM.CKM_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new NullDigest()),
            CKM.CKM_SHA1_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha1Digest()),
            CKM.CKM_SHA224_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha224Digest()),
            CKM.CKM_SHA256_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha256Digest()),
            CKM.CKM_SHA384_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha384Digest()),
            CKM.CKM_SHA512_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha512Digest()),

            CKM.CKM_ECDSA => this.CreateEcdsaSigner(ckMechanism, new NullDigest()),
            CKM.CKM_ECDSA_SHA1 => this.CreateEcdsaSigner(ckMechanism, new Sha1Digest()),
            CKM.CKM_ECDSA_SHA224 => this.CreateEcdsaSigner(ckMechanism, new Sha224Digest()),
            CKM.CKM_ECDSA_SHA256 => this.CreateEcdsaSigner(ckMechanism, new Sha256Digest()),
            CKM.CKM_ECDSA_SHA384 => this.CreateEcdsaSigner(ckMechanism, new Sha384Digest()),
            CKM.CKM_ECDSA_SHA512 => this.CreateEcdsaSigner(ckMechanism, new Sha512Digest()),

            //TODO: Implement CKM_CMS_SIG 

            CKM.CKM_MD2_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new MD2Digest(), null),
            CKM.CKM_MD5_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new MD5Digest(), CKK.CKK_MD5_HMAC),
            CKM.CKM_RIPEMD128_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new RipeMD128Digest(), CKK.CKK_RIPEMD128_HMAC),
            CKM.CKM_RIPEMD160_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new RipeMD160Digest(), CKK.CKK_RIPEMD160_HMAC),
            CKM.CKM_SHA_1_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha1Digest(), CKK.CKK_SHA_1_HMAC),
            CKM.CKM_SHA224_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha224Digest(), CKK.CKK_SHA224_HMAC),
            CKM.CKM_SHA256_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha256Digest(), CKK.CKK_SHA256_HMAC),
            CKM.CKM_SHA384_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha384Digest(), CKK.CKK_SHA384_HMAC),
            CKM.CKM_SHA512_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha512Digest(), CKK.CKK_SHA512_HMAC),
            CKM.CKM_SHA512_224_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha512tDigest(224), null),
            CKM.CKM_SHA512_256_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha512tDigest(256), null),

            CKM.CKM_MD2_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new MD2Digest(), null),
            CKM.CKM_MD5_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new MD5Digest(), CKK.CKK_MD5_HMAC),
            CKM.CKM_RIPEMD128_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new RipeMD128Digest(), CKK.CKK_RIPEMD128_HMAC),
            CKM.CKM_RIPEMD160_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new RipeMD160Digest(), CKK.CKK_RIPEMD160_HMAC),
            CKM.CKM_SHA_1_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha1Digest(), CKK.CKK_SHA_1_HMAC),
            CKM.CKM_SHA224_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha224Digest(), CKK.CKK_SHA224_HMAC),
            CKM.CKM_SHA256_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha256Digest(), CKK.CKK_SHA256_HMAC),
            CKM.CKM_SHA384_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha384Digest(), CKK.CKK_SHA384_HMAC),
            CKM.CKM_SHA512_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha512Digest(), CKK.CKK_SHA512_HMAC),
            CKM.CKM_SHA512_224_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha512tDigest(224), null),
            CKM.CKM_SHA512_256_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha512tDigest(256), null),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for signing.")
        };
    }

    private EcdsaWrapperSigner CreateEcdsaSigner(CKM ckMechanism, IDigest digest)
    {
        return new EcdsaWrapperSigner(new DsaDigestSigner(new ECDsaSigner(), digest, PlainDsaEncoding.Instance),
            ckMechanism,
            this.loggerFactory.CreateLogger<EcdsaWrapperSigner>());
    }

    private HmacWrapperSigner CreateHmacWrapperSigner(CKM ckMechanism, IDigest hmacDigest, CKK? additionalKey)
    {
        return new HmacWrapperSigner(hmacDigest,
            ckMechanism,
            additionalKey,
            null,
            this.loggerFactory.CreateLogger<HmacWrapperSigner>());
    }

    private HmacWrapperSigner CreateHmacGeneralWrapperSigner(MechanismValue mechanism, IDigest hmacDigest, CKK? additionalKey)
    {
        this.logger.LogTrace("Entering to CreateHmacGeneralWrapperSigner with {MechanismType}.", (CKM)mechanism.MechanismType);
        try
        {
            CkP_MacGeneralParams generalParams = MessagePack.MessagePackSerializer.Deserialize<CkP_MacGeneralParams>(mechanism.MechanismParamMp);

            if (generalParams.Value == 0)
            {
                this.logger.LogWarning("CK_MacGeneralParams with value 0 for mechanism {MechanismType}. Signa and verify returns nonsensical results.",
                    (CKM)mechanism.MechanismType);
            }

            return new HmacWrapperSigner(hmacDigest,
                (CKM)mechanism.MechanismType,
                additionalKey,
                (int)generalParams.Value,
                this.loggerFactory.CreateLogger<HmacWrapperSigner>());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds {MechanismType} from parameter.", (CKM)mechanism.MechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    //see https://github.com/jariq/Pkcs7SignatureGenerator/blob/master/src/Pkcs7SignatureGenerator/Pkcs7SignatureGenerator.cs#L338
    // https://www.cryptsoft.com/pkcs11doc/v220/structCK__RSA__PKCS__PSS__PARAMS.html
    private RsaWrapperSigner CreateRsaPssSigner(MechanismValue mechanism, IDigest signContentDigest)
    {
        this.logger.LogTrace("Entering to CreateRsaPssSigner.");

        CkP_RsaPkcsPssParams mechanismParams = MessagePack.MessagePackSerializer.Deserialize<CkP_RsaPkcsPssParams>(mechanism.MechanismParamMp);

        CKM pssHashAlgorithm = (CKM)mechanismParams.HashAlg;
        CKG pssMgf = (CKG)mechanismParams.Mgf;

        this.logger.LogDebug("Obtained CK_RSA_PKCS_PSS_PARAMS with hashAlg {pssHashAlg}, mgf {pssMgf}, sLen {pssSLen}.",
            pssHashAlgorithm,
            pssMgf,
            mechanismParams.SLen);

        IDigest? contentDigest = DigestUtils.TryGetDigest(pssHashAlgorithm);
        if (contentDigest == null)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid digest ({pssHashAlgorithm}) in CK_RSA_PKCS_PSS_PARAMS (mechanism CKM_RSA_PKCS_PSS).");
        }
        IDigest? mgfDigest = DigestUtils.TryGetDigest(pssMgf);
        if (mgfDigest == null)
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid mgf {pssMgf} in CK_RSA_PKCS_PSS_PARAMS (mechanism CKM_RSA_PKCS_PSS).");
        }

        ISigner signer;
        if (signContentDigest is NullDigest)
        {
            signer = PssSigner.CreateRawSigner(new RsaBlindedEngine(),
               contentDigest,
               mgfDigest,
               (int)mechanismParams.SLen,
               PssSigner.TrailerImplicit);
        }
        else
        {
            if (signContentDigest.GetType() != contentDigest.GetType())
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    "Invalid mechanism param CK_RSA_PKCS_PSS_PARAMS.hashAlg: BouncyHsm required som hash algorithm in mechanism and CK_RSA_PKCS_PSS_PARAMS.hashAlg."
                    + $" Mechanism is {(CKM)mechanism.MechanismType}, hashAlg: {pssHashAlgorithm}.");
            }

            signer = new PssSigner(new RsaBlindedEngine(),
               contentDigest,
               mgfDigest,
               (int)mechanismParams.SLen);
        }

        return new RsaWrapperSigner(signer,
            (CKM)mechanism.MechanismType,
            this.loggerFactory.CreateLogger<RsaWrapperSigner>());
    }
}
