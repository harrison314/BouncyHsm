using BouncyHsm.Core.Rpc;
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
using Org.BouncyCastle.Asn1.X509;
using Org.BouncyCastle.Asn1;
using Org.BouncyCastle.Asn1.Nist;
using Org.BouncyCastle.Crypto.Macs;
using System.Net.Http.Headers;

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
            CKM.CKM_RSA_PKCS => new RsaWrapperSigner(this.CreateRsaPkcsSigner(), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_SHA1_RSA_PKCS => this.CreateHashRsaPkcsSigner(ckMechanism, new Sha1Digest(), X509ObjectIdentifiers.IdSha1),
            CKM.CKM_SHA224_RSA_PKCS => this.CreateHashRsaPkcsSigner(ckMechanism, new Sha224Digest(), NistObjectIdentifiers.IdSha224),
            CKM.CKM_SHA256_RSA_PKCS => this.CreateHashRsaPkcsSigner(ckMechanism, new Sha256Digest(), NistObjectIdentifiers.IdSha256),
            CKM.CKM_SHA384_RSA_PKCS => this.CreateHashRsaPkcsSigner(ckMechanism, new Sha384Digest(), NistObjectIdentifiers.IdSha384),
            CKM.CKM_SHA512_RSA_PKCS => this.CreateHashRsaPkcsSigner(ckMechanism, new Sha512Digest(), NistObjectIdentifiers.IdSha512),
            CKM.CKM_MD2_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("MD2withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_MD5_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("MD5withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_RIPEMD128_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("RIPEMD128withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_RIPEMD160_RSA_PKCS => new RsaWrapperSigner(SignerUtilities.GetSigner("RIPEMD160withRSA"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            CKM.CKM_SHA3_224_RSA_PKCS => this.CreateHashRsaPkcsSigner(ckMechanism, new Sha3Digest(224), NistObjectIdentifiers.IdSha3_224),
            CKM.CKM_SHA3_256_RSA_PKCS => this.CreateHashRsaPkcsSigner(ckMechanism, new Sha3Digest(256), NistObjectIdentifiers.IdSha3_256),
            CKM.CKM_SHA3_384_RSA_PKCS => this.CreateHashRsaPkcsSigner(ckMechanism, new Sha3Digest(384), NistObjectIdentifiers.IdSha3_384),
            CKM.CKM_SHA3_512_RSA_PKCS => this.CreateHashRsaPkcsSigner(ckMechanism, new Sha3Digest(512), NistObjectIdentifiers.IdSha3_512),

            CKM.CKM_SHA1_RSA_X9_31 => new RsaWrapperSigner(SignerUtilities.GetSigner("SHA1withRSA/X9.31"), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),
            //CKM.CKM_RSA_X9_31 => new RsaWrapperSigner(new X931Signer(new RsaBlindedEngine(), new NullDigest(), true), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),

            CKM.CKM_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new NullDigest()),
            CKM.CKM_SHA1_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha1Digest()),
            CKM.CKM_SHA224_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha224Digest()),
            CKM.CKM_SHA256_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha256Digest()),
            CKM.CKM_SHA384_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha384Digest()),
            CKM.CKM_SHA512_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha512Digest()),
            CKM.CKM_SHA3_224_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha3Digest(224)),
            CKM.CKM_SHA3_256_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha3Digest(256)),
            CKM.CKM_SHA3_384_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha3Digest(384)),
            CKM.CKM_SHA3_512_RSA_PKCS_PSS => this.CreateRsaPssSigner(mechanism, new Sha3Digest(512)),

            CKM.CKM_ECDSA => this.CreateEcdsaSigner(ckMechanism, new NullDigest()),
            CKM.CKM_ECDSA_SHA1 => this.CreateEcdsaSigner(ckMechanism, new Sha1Digest()),
            CKM.CKM_ECDSA_SHA224 => this.CreateEcdsaSigner(ckMechanism, new Sha224Digest()),
            CKM.CKM_ECDSA_SHA256 => this.CreateEcdsaSigner(ckMechanism, new Sha256Digest()),
            CKM.CKM_ECDSA_SHA384 => this.CreateEcdsaSigner(ckMechanism, new Sha384Digest()),
            CKM.CKM_ECDSA_SHA512 => this.CreateEcdsaSigner(ckMechanism, new Sha512Digest()),
            CKM.CKM_ECDSA_SHA3_224 => this.CreateEcdsaSigner(ckMechanism, new Sha3Digest(224)),
            CKM.CKM_ECDSA_SHA3_256 => this.CreateEcdsaSigner(ckMechanism, new Sha3Digest(256)),
            CKM.CKM_ECDSA_SHA3_384 => this.CreateEcdsaSigner(ckMechanism, new Sha3Digest(384)),
            CKM.CKM_ECDSA_SHA3_512 => this.CreateEcdsaSigner(ckMechanism, new Sha3Digest(512)),

            CKM.CKM_EDDSA => this.CreateEddsaSigner(mechanism),

            CKM.CKM_RSA_9796 => new RsaWrapperSigner(new Iso9796Signer(new RsaBlindedEngine(), new NullDigest()), ckMechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>()),

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
            CKM.CKM_GOSTR3411_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Gost3411Digest(), null),
            CKM.CKM_SHA3_224_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha3Digest(224), CKK.CKK_SHA3_224_HMAC),
            CKM.CKM_SHA3_256_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha3Digest(256), CKK.CKK_SHA3_256_HMAC),
            CKM.CKM_SHA3_384_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha3Digest(384), CKK.CKK_SHA3_384_HMAC),
            CKM.CKM_SHA3_512_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Sha3Digest(512), CKK.CKK_SHA3_512_HMAC),
            CKM.CKM_BLAKE2B_160_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Blake2bDigest(160), CKK.CKK_BLAKE2B_160_HMAC),
            CKM.CKM_BLAKE2B_256_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Blake2bDigest(256), CKK.CKK_BLAKE2B_256_HMAC),
            CKM.CKM_BLAKE2B_384_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Blake2bDigest(384), CKK.CKK_BLAKE2B_384_HMAC),
            CKM.CKM_BLAKE2B_512_HMAC => this.CreateHmacWrapperSigner(ckMechanism, new Blake2bDigest(512), CKK.CKK_BLAKE2B_512_HMAC),
            CKM.CKM_AES_CMAC => this.CreateAesWrapperSigner(ckMechanism, new CMac(AesUtilities.CreateEngine())),

            CKM.CKM_POLY1305 => new PlainMacWrapperSigner(ckMechanism, new Org.BouncyCastle.Crypto.Macs.Poly1305(), this.loggerFactory.CreateLogger<PlainMacWrapperSigner>(), CKK.CKK_POLY1305),

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
            CKM.CKM_SHA3_224_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha3Digest(224), CKK.CKK_SHA3_224_HMAC),
            CKM.CKM_SHA3_256_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha3Digest(256), CKK.CKK_SHA3_256_HMAC),
            CKM.CKM_SHA3_384_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha3Digest(384), CKK.CKK_SHA3_384_HMAC),
            CKM.CKM_SHA3_512_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Sha3Digest(512), CKK.CKK_SHA3_512_HMAC),
            CKM.CKM_BLAKE2B_160_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Blake2bDigest(160), CKK.CKK_BLAKE2B_160_HMAC),
            CKM.CKM_BLAKE2B_256_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Blake2bDigest(256), CKK.CKK_BLAKE2B_256_HMAC),
            CKM.CKM_BLAKE2B_384_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Blake2bDigest(384), CKK.CKK_BLAKE2B_384_HMAC),
            CKM.CKM_BLAKE2B_512_HMAC_GENERAL => this.CreateHmacGeneralWrapperSigner(mechanism, new Blake2bDigest(512), CKK.CKK_BLAKE2B_512_HMAC),

            CKM.CKM_AES_CMAC_GENERAL => this.CreateAesGeneralWrapperSigner(mechanism, new CMac(AesUtilities.CreateEngine())),

            CKM.CKM_ML_DSA => this.CreateMlDsaWrapperSigner(mechanism),
            CKM.CKM_HASH_ML_DSA => this.CreateMlDsaPrehashedWrapperSigner(mechanism),
            CKM.CKM_HASH_ML_DSA_SHA224 => this.CreateHashedMlDsaWrapperSigner(mechanism, new Sha224Digest()),
            CKM.CKM_HASH_ML_DSA_SHA256 => this.CreateHashedMlDsaWrapperSigner(mechanism, new Sha256Digest()),
            CKM.CKM_HASH_ML_DSA_SHA384 => this.CreateHashedMlDsaWrapperSigner(mechanism, new Sha384Digest()),
            CKM.CKM_HASH_ML_DSA_SHA512 => this.CreateHashedMlDsaWrapperSigner(mechanism, new Sha512Digest()),
            CKM.CKM_HASH_ML_DSA_SHA3_224 => this.CreateHashedMlDsaWrapperSigner(mechanism, new Sha3Digest(224)),
            CKM.CKM_HASH_ML_DSA_SHA3_256 => this.CreateHashedMlDsaWrapperSigner(mechanism, new Sha3Digest(256)),
            CKM.CKM_HASH_ML_DSA_SHA3_384 => this.CreateHashedMlDsaWrapperSigner(mechanism, new Sha3Digest(384)),
            CKM.CKM_HASH_ML_DSA_SHA3_512 => this.CreateHashedMlDsaWrapperSigner(mechanism, new Sha3Digest(512)),
            CKM.CKM_HASH_ML_DSA_SHAKE128 => this.CreateHashedMlDsaWrapperSigner(mechanism, new ShakeDigest(128)),
            CKM.CKM_HASH_ML_DSA_SHAKE256 => this.CreateHashedMlDsaWrapperSigner(mechanism, new ShakeDigest(256)),

            CKM.CKM_SLH_DSA => this.CreateSlhDsaWrapperSigner(mechanism),
            CKM.CKM_HASH_SLH_DSA => this.CreateSlhDsaPrehashedWrapperSigner(mechanism),
            CKM.CKM_HASH_SLH_DSA_SHA224 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new Sha224Digest()),
            CKM.CKM_HASH_SLH_DSA_SHA256 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new Sha256Digest()),
            CKM.CKM_HASH_SLH_DSA_SHA384 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new Sha384Digest()),
            CKM.CKM_HASH_SLH_DSA_SHA512 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new Sha512Digest()),
            CKM.CKM_HASH_SLH_DSA_SHA3_224 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new Sha3Digest(224)),
            CKM.CKM_HASH_SLH_DSA_SHA3_256 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new Sha3Digest(256)),
            CKM.CKM_HASH_SLH_DSA_SHA3_384 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new Sha3Digest(384)),
            CKM.CKM_HASH_SLH_DSA_SHA3_512 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new Sha3Digest(512)),
            CKM.CKM_HASH_SLH_DSA_SHAKE128 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new ShakeDigest(128)),
            CKM.CKM_HASH_SLH_DSA_SHAKE256 => this.CreateHashedSlhDsaWrapperSigner(mechanism, new ShakeDigest(256)),


            // AES Signers
            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for signing or validation.")
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
            CkP_MacGeneralParams generalParams = MessagePack.MessagePackSerializer.Deserialize<CkP_MacGeneralParams>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

            if (generalParams.Value == 0)
            {
                this.logger.LogWarning("CK_MacGeneralParams with value 0 for mechanism {MechanismType}. Sign and verify returns nonsensical results.",
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

        CkP_RsaPkcsPssParams mechanismParams = MessagePack.MessagePackSerializer.Deserialize<CkP_RsaPkcsPssParams>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

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

    private ISigner CreateRsaPkcsSigner()
    {
        Pkcs1DigestInfoCheckerAsDigest nullDigest = new Pkcs1DigestInfoCheckerAsDigest(this.loggerFactory.CreateLogger<Pkcs1DigestInfoCheckerAsDigest>());

        // Alternative for SignerUtilities.GetSigner("RSA")
        return new RsaDigestSigner(nullDigest, (AlgorithmIdentifier?)null);
    }

    private RsaWrapperSigner CreateHashRsaPkcsSigner(CKM mechanism, IDigest difest, DerObjectIdentifier digestOid)
    {
        RsaDigestSigner signer = new RsaDigestSigner(difest, new AlgorithmIdentifier(digestOid, DerNull.Instance));
        return new RsaWrapperSigner(signer, mechanism, this.loggerFactory.CreateLogger<RsaWrapperSigner>());
    }

    private EdDsaCipherWrapper CreateEddsaSigner(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateEddsaSigner.");

        Ckp_CkEddsaParams? mechanismParams = null;

        if (mechanism.MechanismParamMp != null)
        {
            mechanismParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkEddsaParams>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());
        }

        return new EdDsaCipherWrapper(mechanismParams,
            this.loggerFactory.CreateLogger<EdDsaCipherWrapper>());
    }

    private MlDsaWrapperSigner CreateMlDsaWrapperSigner(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateMlDsaWrapperSigner.");

        Ckp_CkSignAdditionalContext? mechanismParams = null;
        if (mechanism.MechanismParamMp != null)
        {
            mechanismParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkSignAdditionalContext>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());
            this.logger.LogDebug("Obtained CK_SIGN_ADDITIONAL_CONTEXT with hedgeVariant {hedgeVariant}, context length {contextLength}.",
               (CK_HEDGE_TYPE)mechanismParams.HedgeVariant,
               mechanismParams.Context?.Length);

            if (!Enum.IsDefined<CK_HEDGE_TYPE>((CK_HEDGE_TYPE)mechanismParams.HedgeVariant))
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    "Invalid mechanism param CK_SIGN_ADDITIONAL_CONTEXT.hedgeVariant: Invalid value.");
            }
        }

        return new MlDsaWrapperSigner(mechanismParams,
            this.loggerFactory.CreateLogger<MlDsaWrapperSigner>());
    }

    private MlDsaPrehashedWrapperSigner CreateMlDsaPrehashedWrapperSigner(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateMlDsaHashedWrapperSigner.");

        Ckp_CkHashSignAdditionalContext mechanismParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkHashSignAdditionalContext>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());
        this.logger.LogDebug("Obtained CK_HASH_SIGN_ADDITIONAL_CONTEXT with hedgeVariant {hedgeVariant}, context length {contextLength}, hash {hashMechanism}.",
           (CK_HEDGE_TYPE)mechanismParams.HedgeVariant,
           mechanismParams.Context?.Length,
           (CKM)mechanismParams.Hash);

        if (!Enum.IsDefined<CK_HEDGE_TYPE>((CK_HEDGE_TYPE)mechanismParams.HedgeVariant))
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                "Invalid mechanism param CK_SIGN_ADDITIONAL_CONTEXT.hedgeVariant: Invalid value.");
        }

        return new MlDsaPrehashedWrapperSigner(mechanismParams, this.loggerFactory.CreateLogger<MlDsaPrehashedWrapperSigner>()); ;
    }

    private MlDsaHashedWrapperSigner CreateHashedMlDsaWrapperSigner(MechanismValue mechanism, IDigest usedDigest)
    {
        this.logger.LogTrace("Entering to CreateHashedMlDsaWrapperSigner.");

        Ckp_CkSignAdditionalContext? mechanismParams = null;
        if (mechanism.MechanismParamMp != null)
        {
            mechanismParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkSignAdditionalContext>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());
            this.logger.LogDebug("Obtained CK_SIGN_ADDITIONAL_CONTEXT with hedgeVariant {hedgeVariant}, context length {contextLength}.",
               (CK_HEDGE_TYPE)mechanismParams.HedgeVariant,
               mechanismParams.Context?.Length);

            if (!Enum.IsDefined<CK_HEDGE_TYPE>((CK_HEDGE_TYPE)mechanismParams.HedgeVariant))
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    "Invalid mechanism param CK_SIGN_ADDITIONAL_CONTEXT.hedgeVariant: Invalid value.");
            }
        }

        return new MlDsaHashedWrapperSigner((CKM)mechanism.MechanismType,
            mechanismParams,
            usedDigest,
            this.loggerFactory.CreateLogger<MlDsaHashedWrapperSigner>());
    }

    private SlhDsaWrapperSigner CreateSlhDsaWrapperSigner(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateSlhDsaWrapperSigner.");

        Ckp_CkSignAdditionalContext? mechanismParams = null;
        if (mechanism.MechanismParamMp != null)
        {
            mechanismParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkSignAdditionalContext>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());
            this.logger.LogDebug("Obtained CK_SIGN_ADDITIONAL_CONTEXT with hedgeVariant {hedgeVariant}, context length {contextLength}.",
               (CK_HEDGE_TYPE)mechanismParams.HedgeVariant,
               mechanismParams.Context?.Length);

            if (!Enum.IsDefined<CK_HEDGE_TYPE>((CK_HEDGE_TYPE)mechanismParams.HedgeVariant))
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    "Invalid mechanism param CK_SIGN_ADDITIONAL_CONTEXT.hedgeVariant: Invalid value.");
            }
        }

        return new SlhDsaWrapperSigner(mechanismParams,
            this.loggerFactory.CreateLogger<SlhDsaWrapperSigner>());
    }

    private SlhDsaPrehashedWrapperSigner CreateSlhDsaPrehashedWrapperSigner(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateSlhDsaPrehashedWrapperSigner.");

        Ckp_CkHashSignAdditionalContext mechanismParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkHashSignAdditionalContext>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());
        this.logger.LogDebug("Obtained CK_HASH_SIGN_ADDITIONAL_CONTEXT with hedgeVariant {hedgeVariant}, context length {contextLength}, hash {hashMechanism}.",
           (CK_HEDGE_TYPE)mechanismParams.HedgeVariant,
           mechanismParams.Context?.Length,
           (CKM)mechanismParams.Hash);

        if (!Enum.IsDefined<CK_HEDGE_TYPE>((CK_HEDGE_TYPE)mechanismParams.HedgeVariant))
        {
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                "Invalid mechanism param CK_SIGN_ADDITIONAL_CONTEXT.hedgeVariant: Invalid value.");
        }

        return new SlhDsaPrehashedWrapperSigner(mechanismParams, this.loggerFactory.CreateLogger<SlhDsaPrehashedWrapperSigner>()); ;
    }

    private SlhDsaHashedWrapperSigner CreateHashedSlhDsaWrapperSigner(MechanismValue mechanism, IDigest usedDigest)
    {
        this.logger.LogTrace("Entering to CreateHashedSlhDsaWrapperSigner.");

        Ckp_CkSignAdditionalContext? mechanismParams = null;
        if (mechanism.MechanismParamMp != null)
        {
            mechanismParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkSignAdditionalContext>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());
            this.logger.LogDebug("Obtained CK_SIGN_ADDITIONAL_CONTEXT with hedgeVariant {hedgeVariant}, context length {contextLength}.",
               (CK_HEDGE_TYPE)mechanismParams.HedgeVariant,
               mechanismParams.Context?.Length);

            if (!Enum.IsDefined<CK_HEDGE_TYPE>((CK_HEDGE_TYPE)mechanismParams.HedgeVariant))
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID,
                    "Invalid mechanism param CK_SIGN_ADDITIONAL_CONTEXT.hedgeVariant: Invalid value.");
            }
        }

        return new SlhDsaHashedWrapperSigner((CKM)mechanism.MechanismType,
            mechanismParams,
            usedDigest,
            this.loggerFactory.CreateLogger<SlhDsaHashedWrapperSigner>());
    }

    private AesWrapperSigner CreateAesWrapperSigner(CKM mechanismType, IMac mac)
    {
        return new AesWrapperSigner(mechanismType,
            mac,
            null,
            this.loggerFactory.CreateLogger<AesWrapperSigner>());
    }

    private AesWrapperSigner CreateAesGeneralWrapperSigner(MechanismValue mechanism, IMac mac)
    {
        this.logger.LogTrace("Entering to CreateAesGeneralWrapperSigner with {MechanismType}.", (CKM)mechanism.MechanismType);
        try
        {
            CkP_MacGeneralParams generalParams = MessagePack.MessagePackSerializer.Deserialize<CkP_MacGeneralParams>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

            if (generalParams.Value == 0)
            {
                this.logger.LogWarning("CK_MacGeneralParams with value 0 for mechanism {MechanismType}. Sign and verify returns nonsensical results.",
                    (CKM)mechanism.MechanismType);
            }

            return new AesWrapperSigner((CKM)mechanism.MechanismType,
             mac,
             (int)generalParams.Value,
             this.loggerFactory.CreateLogger<AesWrapperSigner>());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds {MechanismType} from parameter.", (CKM)mechanism.MechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }
}
