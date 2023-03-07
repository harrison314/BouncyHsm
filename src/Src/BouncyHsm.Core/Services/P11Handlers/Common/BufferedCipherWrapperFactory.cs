using BouncyHsm.Core.Rpc;
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

    public IBufferedCipherWrapper CreateCipherAlgorithm(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateCipherAlgorithm with mechanism type {mechanismType}", mechanism.MechanismType);


        CKM ckMechanism = (CKM)mechanism.MechanismType;

        return ckMechanism switch
        {
            CKM.CKM_AES_ECB => this.CreateAesWithoutIv(CipherUtilities.GetCipher("AES/ECB/NOPADDING"), mechanism), //TODO: padding question
            CKM.CKM_AES_CBC => this.CreateAes(CipherUtilities.GetCipher("AES/CBC/NOPADDING"), mechanism),
            CKM.CKM_AES_CBC_PAD => this.CreateAes(CipherUtilities.GetCipher("AES/CBC/PKCS7PADDING"), mechanism),

            CKM.CKM_AES_CFB1 => this.CreateAes(CipherUtilities.GetCipher("AES/CFB/NOPADDING"), mechanism), //TODO: check with specification
            CKM.CKM_AES_CFB8 => this.CreateAes(CipherUtilities.GetCipher("AES/CFB8/NOPADDING"), mechanism),
            CKM.CKM_AES_CFB64 => this.CreateAes(CipherUtilities.GetCipher("AES/CFB64/NOPADDING"), mechanism),
            CKM.CKM_AES_CFB128 => this.CreateAes(CipherUtilities.GetCipher("AES/CFB128/NOPADDING"), mechanism),

            CKM.CKM_AES_OFB => this.CreateAes(CipherUtilities.GetCipher("AES/OFB/NOPADDING"), mechanism),
            CKM.CKM_AES_CTR => this.CreateAes(CipherUtilities.GetCipher("AES/CTR/NOPADDING"), mechanism),
            CKM.CKM_AES_CTS => this.CreateAes(CipherUtilities.GetCipher("AES/CTS/NOPADDING"), mechanism),

            CKM.CKM_AES_GCM => this.CreateAesGcm(CipherUtilities.GetCipher("AES/GCM/NOPADDING"), mechanism),
            CKM.CKM_AES_CCM => this.CreateAesCcm(CipherUtilities.GetCipher("AES/CCM/NOPADDING"), mechanism),

            CKM.CKM_RSA_PKCS => this.CreateRsaPkcs(mechanism),
            CKM.CKM_RSA_PKCS_OAEP => this.CreateRsaOaep(mechanism),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for encrypt, decrypt, wrap or unwrap.")
        };
    }

    private AesBufferedCipherWrapper CreateAesWithoutIv(IBufferedCipher bufferedCipher, MechanismValue mechanism)
    {
        return new AesBufferedCipherWrapper(bufferedCipher,
            null,
            (CKM)mechanism.MechanismType,
            this.loggerFactory.CreateLogger<AesBufferedCipherWrapper>());
    }

    private AesBufferedCipherWrapper CreateAes(IBufferedCipher bufferedCipher, MechanismValue mechanism)
    {
        try
        {
            CkP_RawDataParams rawDataParams = MessagePack.MessagePackSerializer.Deserialize<CkP_RawDataParams>(mechanism.MechanismParamMp);

            this.logger.LogDebug("Extract IV with len {ivLen} for mechanism {mechanism}.",
                rawDataParams.Value.Length,
                (CKM)mechanism.MechanismType);

            return new AesBufferedCipherWrapper(bufferedCipher,
                rawDataParams.Value,
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
            Ckp_CkGcmParams gcmParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkGcmParams>(mechanism.MechanismParamMp);

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
                this.logger.LogWarning("Ignore IvBits in CkGcmParams. IV has {ivLen} bit lenght, iv bits is {ivBits}.",
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
            Ckp_CkCcmParams ccmParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkCcmParams>(mechanism.MechanismParamMp);

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
            Ckp_CkRsaPkcsOaepParams rsaPkcsOaepParamas = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkRsaPkcsOaepParams>(mechanism.MechanismParamMp);

            if (this.logger.IsEnabled(LogLevel.Trace))
            {
                this.logger.LogTrace("Using RSA OAEP params with hashAlg {hashAlg}, mgf {mgf}, source {source}, source data len {sourceDataLen}.",
                    (CKM)rsaPkcsOaepParamas.HashAlg,
                    (CKG)rsaPkcsOaepParamas.Mgf,
                    (CKZ)rsaPkcsOaepParamas.Source,
                    rsaPkcsOaepParamas.SourceData?.Length ?? 0);
            }


            IDigest? hashAlg = DigestUtils.TryGetDigest((CKM)rsaPkcsOaepParamas.HashAlg);
            if (hashAlg == null)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid hashAlg {(CKM)rsaPkcsOaepParamas.Mgf} in CK_RSA_PKCS_OAEP_PARAMS (mechanism CKM_RSA_PKCS_OAEP).");
            }

            IDigest? mgf = DigestUtils.TryGetDigest((CKG)rsaPkcsOaepParamas.Mgf);
            if (mgf == null)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid mgf {(CKG)rsaPkcsOaepParamas.Mgf} in CK_RSA_PKCS_OAEP_PARAMS (mechanism CKM_RSA_PKCS_OAEP).");
            }

            RsaBlindedEngine rsa = new RsaBlindedEngine();
            OaepEncoding rsaOpeap = new OaepEncoding(rsa, hashAlg, mgf, rsaPkcsOaepParamas.SourceData);
            BufferedAsymmetricBlockCipher bufferedChiper = new BufferedAsymmetricBlockCipher(rsaOpeap);

            return new RsaBufferedCipherWrapper(bufferedChiper,
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

}
