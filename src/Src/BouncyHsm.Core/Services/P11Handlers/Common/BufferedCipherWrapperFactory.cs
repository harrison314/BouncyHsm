using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto;
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

            //https://www.cryptsoft.com/pkcs11doc/v230/group__SEC__11__11__2__AES__GCM__AND__CCM__MECHANISM__PARAMETERS.html
            //CKM.CKM_AES_GCM => this.CreateAes(CipherUtilities.GetCipher("AES/GCM/NOPADDING"), mechanism),

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
        CkP_RawDataParams rawDataParams = MessagePack.MessagePackSerializer.Deserialize<CkP_RawDataParams>(mechanism.MechanismParamMp);

        this.logger.LogDebug("Extract IV with len {ivLen} for mechanism {mechanism}.",
            rawDataParams.Value.Length,
            (CKM)mechanism.MechanismType);

        return new AesBufferedCipherWrapper(bufferedCipher,
            rawDataParams.Value,
            (CKM)mechanism.MechanismType,
            this.loggerFactory.CreateLogger<AesBufferedCipherWrapper>());
    }
}
