﻿using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Encodings;
using Org.BouncyCastle.Crypto.Engines;
using Org.BouncyCastle.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal class BufferedCipherWrapperFactory
{
    private readonly ILogger<BufferedCipherWrapperFactory> logger;
    private readonly ILoggerFactory loggerFactory;

    public BufferedCipherWrapperFactory(ILoggerFactory loggerFactory)
    {
        this.logger = loggerFactory.CreateLogger<BufferedCipherWrapperFactory>();
        this.loggerFactory = loggerFactory;
    }

    public ICipherWrapper CreateCipherAlgorithm(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateCipherAlgorithm with mechanism type {mechanismType}", mechanism.MechanismType);


        CKM ckMechanism = (CKM)mechanism.MechanismType;

        return ckMechanism switch
        {
            CKM.CKM_AES_ECB => this.CreateAesWithoutIv(CipherUtilities.GetCipher("AES/ECB/NOPADDING"), mechanism), //TODO: padding question
            CKM.CKM_AES_CBC => this.CreateAes(CipherUtilities.GetCipher("AES/CBC/NOPADDING"), true, mechanism),
            CKM.CKM_AES_CBC_PAD => this.CreateAes(CipherUtilities.GetCipher("AES/CBC/PKCS7PADDING"), false, mechanism),

            CKM.CKM_AES_CFB1 => this.CreateAes(CipherUtilities.GetCipher("AES/CFB/NOPADDING"), true, mechanism), //TODO: check with specification
            CKM.CKM_AES_CFB8 => this.CreateAes(CipherUtilities.GetCipher("AES/CFB8/NOPADDING"), true, mechanism),
            CKM.CKM_AES_CFB64 => this.CreateAes(CipherUtilities.GetCipher("AES/CFB64/NOPADDING"), true, mechanism),
            CKM.CKM_AES_CFB128 => this.CreateAes(CipherUtilities.GetCipher("AES/CFB128/NOPADDING"), true, mechanism),

            CKM.CKM_AES_OFB => this.CreateAes(CipherUtilities.GetCipher("AES/OFB/NOPADDING"), true, mechanism),
            CKM.CKM_AES_CTR => this.CreateAes(CipherUtilities.GetCipher("AES/CTR/NOPADDING"), true, mechanism),
            CKM.CKM_AES_CTS => this.CreateAes(CipherUtilities.GetCipher("AES/CTS/NOPADDING"), true, mechanism),

            CKM.CKM_AES_GCM => this.CreateAesGcm(CipherUtilities.GetCipher("AES/GCM/NOPADDING"), mechanism),
            CKM.CKM_AES_CCM => this.CreateAesCcm(CipherUtilities.GetCipher("AES/CCM/NOPADDING"), mechanism),

            CKM.CKM_RSA_PKCS => this.CreateRsaPkcs(mechanism),
            CKM.CKM_RSA_PKCS_OAEP => this.CreateRsaOaep(mechanism),

            CKM.CKM_AES_KEY_WRAP_PAD => this.CreateAesKeyWrap(ckMechanism),

            CKM.CKM_CHACHA20 => this.CreateChaCha20(mechanism),
            CKM.CKM_CHACHA20_POLY1305 => this.CreateChaCha20Poly1305(mechanism),

            CKM.CKM_SALSA20 => this.CreateSalsa20(mechanism),


            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for encrypt, decrypt, wrap or unwrap.")
        };
    }

    private AesBufferedCipherWrapper CreateAesWithoutIv(IBufferedCipher bufferedCipher, MechanismValue mechanism)
    {
        return new AesBufferedCipherWrapper(bufferedCipher,
            null,
            true,
            (CKM)mechanism.MechanismType,
            this.loggerFactory.CreateLogger<AesBufferedCipherWrapper>());
    }

    private AesBufferedCipherWrapper CreateAes(IBufferedCipher bufferedCipher, bool padZeroForWrap, MechanismValue mechanism)
    {
        try
        {
            CkP_RawDataParams rawDataParams = MessagePack.MessagePackSerializer.Deserialize<CkP_RawDataParams>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

            this.logger.LogDebug("Extract IV with len {ivLen} for mechanism {mechanism}.",
                rawDataParams.Value.Length,
                (CKM)mechanism.MechanismType);

            return new AesBufferedCipherWrapper(bufferedCipher,
                rawDataParams.Value,
                padZeroForWrap,
                (CKM)mechanism.MechanismType,
                this.loggerFactory.CreateLogger<AesBufferedCipherWrapper>());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds {MechanismType} from parameter.", (CKM)mechanism.MechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private AesAeadBufferedCipherWrapper CreateAesGcm(IBufferedCipher bufferedCipher, MechanismValue mechanism)
    {
        try
        {
            Ckp_CkGcmParams gcmParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkGcmParams>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace("Using AES GCM params with IV len {ivLen}, IV bits {ivBits}, AAD len {aadLen}, tag bits {tagBits}.",
                    gcmParams.Iv?.Length ?? 0,
                    gcmParams.IvBits,
                    gcmParams.Aad?.Length ?? 0,
                    gcmParams.TagBits);
            }

            if (gcmParams.IvBits != 0 && gcmParams.Iv != null && gcmParams.Iv.Length * 8 != gcmParams.IvBits)
            {
                this.logger.LogWarning("Ignore IvBits in CkGcmParams. IV has {ivLen} bit length, iv bits is {ivBits}.",
                    gcmParams.Iv.Length * 8,
                    gcmParams.IvBits);
            }

            return new AesAeadBufferedCipherWrapper(bufferedCipher,
                (int)gcmParams.TagBits,
                gcmParams.Iv,
                gcmParams.Aad,
                (CKM)mechanism.MechanismType,
                this.loggerFactory.CreateLogger<AesAeadBufferedCipherWrapper>());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds {MechanismType} from parameter.", (CKM)mechanism.MechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private AesAeadBufferedCipherWrapper CreateAesCcm(IBufferedCipher bufferedCipher, MechanismValue mechanism)
    {
        try
        {
            Ckp_CkCcmParams ccmParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkCcmParams>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace("Using AES CCM params with nonce len {nonceLen}, AAD len {aadLen}, mac len {macLen}.",
                    ccmParams.Nonce?.Length ?? 0,
                    ccmParams.Aad?.Length ?? 0,
                    ccmParams.MacLen);
            }

            return new AesAeadBufferedCipherWrapper(bufferedCipher,
                (int)ccmParams.MacLen,
                ccmParams.Nonce,
                ccmParams.Aad,
                (CKM)mechanism.MechanismType,
                this.loggerFactory.CreateLogger<AesAeadBufferedCipherWrapper>());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds {MechanismType} from parameter.", (CKM)mechanism.MechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private RsaBufferedCipherWrapper CreateRsaPkcs(MechanismValue mechanism)
    {
        return new RsaBufferedCipherWrapper(CipherUtilities.GetCipher("RSA//PKCS1PADDING"),
            (CKM)mechanism.MechanismType,
            this.loggerFactory.CreateLogger<RsaBufferedCipherWrapper>());
    }

    private RsaBufferedCipherWrapper CreateRsaOaep(MechanismValue mechanism)
    {
        try
        {
            Ckp_CkRsaPkcsOaepParams rsaPkcsOaepParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkRsaPkcsOaepParams>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace("Using RSA OAEP params with hashAlg {hashAlg}, mgf {mgf}, source {source}, source data len {sourceDataLen}.",
                    (CKM)rsaPkcsOaepParams.HashAlg,
                    (CKG)rsaPkcsOaepParams.Mgf,
                    (CKZ)rsaPkcsOaepParams.Source,
                    rsaPkcsOaepParams.SourceData?.Length ?? 0);
            }


            IDigest? hashAlg = DigestUtils.TryGetDigest((CKM)rsaPkcsOaepParams.HashAlg);
            if (hashAlg == null)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid hashAlg {(CKM)rsaPkcsOaepParams.Mgf} in CK_RSA_PKCS_OAEP_PARAMS (mechanism CKM_RSA_PKCS_OAEP).");
            }

            IDigest? mgf = DigestUtils.TryGetDigest((CKG)rsaPkcsOaepParams.Mgf);
            if (mgf == null)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid mgf {(CKG)rsaPkcsOaepParams.Mgf} in CK_RSA_PKCS_OAEP_PARAMS (mechanism CKM_RSA_PKCS_OAEP).");
            }

            RsaBlindedEngine rsa = new RsaBlindedEngine();
            OaepEncoding rsaOpeap = new OaepEncoding(rsa, hashAlg, mgf, rsaPkcsOaepParams.SourceData);
            BufferedAsymmetricBlockCipher bufferedCipher = new BufferedAsymmetricBlockCipher(rsaOpeap);

            return new RsaBufferedCipherWrapper(bufferedCipher,
                (CKM)mechanism.MechanismType,
                this.loggerFactory.CreateLogger<RsaBufferedCipherWrapper>());
        }
        catch (RpcPkcs11Exception)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds {MechanismType} from parameter.", (CKM)mechanism.MechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private Rfc5649BufferedCipherWrapper CreateAesKeyWrap(CKM ckMechanism)
    {
        return new Rfc5649BufferedCipherWrapper(AesUtilities.CreateEngine(),
            ckMechanism,
            this.loggerFactory.CreateLogger<Rfc5649BufferedCipherWrapper>());
    }

    private ICipherWrapper CreateChaCha20(MechanismValue mechanism)
    {
        try
        {
            Ckp_CkChaCha20Params chaCha20params = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkChaCha20Params>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace("Using CHACHA20_PARAMS params with blockCounter {blockCounter}, blockCounterBits {blockCounterBits}, is blockCounterSet {isBlockCounterSet}, nonce len {nonceLen}.",
                    chaCha20params.BlockCounterLower,
                    chaCha20params.BlockCounterBits,
                    chaCha20params.BlockCounterIsSet,
                    chaCha20params.Nonce.Length);
            }

            if (!chaCha20params.BlockCounterIsSet)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType} - pBlockCounter is NULL.");
            }

            if (chaCha20params.BlockCounterBits != 32 && chaCha20params.BlockCounterBits != 64)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType} - blockCounterBits must be 32 or 64.");
            }

            // BouncyHsm implementation errors
            if (chaCha20params.BlockCounterLower != 0 || chaCha20params.BlockCounterUpper != 0)
            {
                throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, $"BouncyHsm accept only value 0 for blockCounter in CHACHA20_PARAMS for mechanism {(CKM)mechanism.MechanismType} yet.");
            }

            if (chaCha20params.Nonce.Length != ChaCha20CipherWrapper.Chacha20NonceSize
                && chaCha20params.Nonce.Length != ChaCha20CipherWrapper.Chacha20_7539NonceSize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, $"BouncyHsm accept only 64 and 96 bits nonce length in CHACHA20_PARAMS for mechanism {(CKM)mechanism.MechanismType} yet.");
            }

            return new ChaCha20CipherWrapper(chaCha20params.Nonce,
                (CKM)mechanism.MechanismType,
                this.loggerFactory.CreateLogger<ChaCha20CipherWrapper>());
        }
        catch (RpcPkcs11Exception)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds {MechanismType} from parameter.", (CKM)mechanism.MechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private ICipherWrapper CreateChaCha20Poly1305(MechanismValue mechanism)
    {
        try
        {
            Ckp_CkSalsa20ChaCha20Poly1305Params chaCha20Poly1305params = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkSalsa20ChaCha20Poly1305Params>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace("Using CK_SALSA20_CHACHA20_POLY1305_PARAMS params with Nonce len {nonceLen}, AAD data len {aadDataLen}.",
                    chaCha20Poly1305params.Nonce.Length,
                    chaCha20Poly1305params.AadData?.Length);
            }

            // BouncyHsm implementation errors
            if (chaCha20Poly1305params.Nonce.Length != 12)
            {
                throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, $"BouncyHsm accept only 96 bits nonce CK_SALSA20_CHACHA20_POLY1305_PARAMS for mechanism {(CKM)mechanism.MechanismType} yet.");
            }

            return new ChaCha20Poly1305CipherWrapper(chaCha20Poly1305params.Nonce,
                chaCha20Poly1305params.AadData,
                (CKM)mechanism.MechanismType,
                this.loggerFactory.CreateLogger<ChaCha20Poly1305CipherWrapper>());
        }
        catch (RpcPkcs11Exception)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds {MechanismType} from parameter.", (CKM)mechanism.MechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private ICipherWrapper CreateSalsa20(MechanismValue mechanism)
    {
        try
        {
            Ckp_CkSalsa20Params salsa20params = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkSalsa20Params>(mechanism.MechanismParamMp, MessagepackBouncyHsmResolver.GetOptions());

            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace("Using SALSA20_PARAMS params with blockCounter {blockCounter}, is blockCounterSet {isBlockCounterSet}, nonce len {nonceLen}.",
                    salsa20params.BlockCounter,
                    salsa20params.BlockCounterIsSet,
                    salsa20params.Nonce.Length);
            }

            if (!salsa20params.BlockCounterIsSet)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType} - pBlockCounter is NULL.");
            }

            if (salsa20params.Nonce.Length != Salsa20CipherWrapper.Salsa20NonceSize
                && salsa20params.Nonce.Length != Salsa20CipherWrapper.XSalsa20NonceSize)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"BouncyHsm accept only 64 and 192 bits nonce length in SALSA20_PARAMS for mechanism {(CKM)mechanism.MechanismType} yet.");
            }

            // BouncyHsm implementation errors
            if (salsa20params.BlockCounter != 0UL)
            {
                throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, $"BouncyHsm accept only value 0 for blockCounter in SALSA20_PARAMS for mechanism {(CKM)mechanism.MechanismType} yet.");
            }

            return new Salsa20CipherWrapper(salsa20params.Nonce,
                (CKM)mechanism.MechanismType,
                this.loggerFactory.CreateLogger<Salsa20CipherWrapper>());
        }
        catch (RpcPkcs11Exception)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in builds {MechanismType} from parameter.", (CKM)mechanism.MechanismType);
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }
}
