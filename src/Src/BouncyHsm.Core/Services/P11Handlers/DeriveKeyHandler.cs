using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Digests;
using Org.BouncyCastle.Security;
using System.Threading;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class DeriveKeyHandler : IRpcRequestHandler<DeriveKeyRequest, DeriveKeyEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<DeriveKeyHandler> logger;

    public DeriveKeyHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<DeriveKeyHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async ValueTask<DeriveKeyEnvelope> Handle(DeriveKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        if (!memorySession.IsUserLogged(p11Session.SlotId))
        {
            throw new RpcPkcs11Exception(CKR.CKR_USER_NOT_LOGGED_IN, "CreateObject requires login");
        }

        if (!p11Session.IsRwSession)
        {
            throw new RpcPkcs11Exception(CKR.CKR_SESSION_READ_ONLY, "CreateObject requires read-write session");
        }

        Dictionary<CKA, IAttributeValue> keyTemplate = AttrTypeUtils.BuildDictionaryTemplate(request.Template);

        KeyObject baseKeyObject = await this.hwServices.FindObjectByHandle<KeyObject>(memorySession,
            p11Session,
            request.BaseKeyHandle,
            cancellationToken);

        if (!baseKeyObject.CkaDerive)
        {
            throw new RpcPkcs11Exception(CKR.CKR_KEY_FUNCTION_NOT_PERMITTED, "Object can not enable drive.");
        }

        IDeriveKeyGenerator generator = await this.CreateDeriveKeyGenerator(request.Mechanism, memorySession, p11Session);
        generator.Init(keyTemplate);
        SecretKeyObject keyObject = generator.Generate(baseKeyObject);

        keyObject.ReComputeAttributes();
        keyObject.Validate();

        uint handle = await this.hwServices.StoreObject(memorySession,
            p11Session,
            keyObject,
            cancellationToken);

        if (this.logger.IsEnabled(LogLevel.Information))
        {
            this.logger.LogInformation("Create new symmetric key using {generator}. Key <Id: {publicKeyId}, CK_ID: {publicKeyCkId}, CK_LABEL: {publicKeyCkLabel}>",
                generator.ToString(),
                keyObject.Id,
                Convert.ToHexString(keyObject.CkaId),
                keyObject.CkaLabel);
        }

        return new DeriveKeyEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new DeriveKeyData()
            {
                KeyHandle = handle
            }
        };
    }

    private async ValueTask<IDeriveKeyGenerator> CreateDeriveKeyGenerator(MechanismValue mechanism, IMemorySession memorySession, IP11Session p11Session)
    {
        this.logger.LogTrace("Entering to CreateDeriveKeyGenerator with mechanism type {mechanismType}", (CKM)mechanism.MechanismType);

        MechanismUtils.CheckMechanism(mechanism, MechanismCkf.CKF_DERIVE);
        CKM ckMechanism = (CKM)mechanism.MechanismType;

        return ckMechanism switch
        {
            CKM.CKM_MD2_KEY_DERIVATION => new DigestDeriveKeyGenerator(new MD2Digest(), this.loggerFactory.CreateLogger<DigestDeriveKeyGenerator>()),
            CKM.CKM_MD5_KEY_DERIVATION => new DigestDeriveKeyGenerator(new MD5Digest(), this.loggerFactory.CreateLogger<DigestDeriveKeyGenerator>()),
            CKM.CKM_SHA1_KEY_DERIVATION => new DigestDeriveKeyGenerator(new Sha1Digest(), this.loggerFactory.CreateLogger<DigestDeriveKeyGenerator>()),
            CKM.CKM_SHA224_KEY_DERIVATION => new DigestDeriveKeyGenerator(new Sha224Digest(), this.loggerFactory.CreateLogger<DigestDeriveKeyGenerator>()),
            CKM.CKM_SHA256_KEY_DERIVATION => new DigestDeriveKeyGenerator(new Sha256Digest(), this.loggerFactory.CreateLogger<DigestDeriveKeyGenerator>()),
            CKM.CKM_SHA384_KEY_DERIVATION => new DigestDeriveKeyGenerator(new Sha384Digest(), this.loggerFactory.CreateLogger<DigestDeriveKeyGenerator>()),
            CKM.CKM_SHA512_KEY_DERIVATION => new DigestDeriveKeyGenerator(new Sha512Digest(), this.loggerFactory.CreateLogger<DigestDeriveKeyGenerator>()),
            CKM.CKM_SHA512_224_KEY_DERIVATION => new DigestDeriveKeyGenerator(new Sha512tDigest(224), this.loggerFactory.CreateLogger<DigestDeriveKeyGenerator>()),
            CKM.CKM_SHA512_256_KEY_DERIVATION => new DigestDeriveKeyGenerator(new Sha512tDigest(256), this.loggerFactory.CreateLogger<DigestDeriveKeyGenerator>()),

            CKM.CKM_CONCATENATE_BASE_AND_DATA => new ConcatBaseAndDataDeriveKeyGenerator(this.GetRawDataParameter(mechanism), this.loggerFactory.CreateLogger<ConcatBaseAndDataDeriveKeyGenerator>()),
            CKM.CKM_CONCATENATE_DATA_AND_BASE => new ConcatDataAndBaseDeriveKeyGenerator(this.GetRawDataParameter(mechanism), this.loggerFactory.CreateLogger<ConcatDataAndBaseDeriveKeyGenerator>()),
            CKM.CKM_XOR_BASE_AND_DATA => new XorBaseAndDataDeriveKeyGenerator(this.GetRawDataParameter(mechanism), this.loggerFactory.CreateLogger<XorBaseAndDataDeriveKeyGenerator>()),
            CKM.CKM_CONCATENATE_BASE_AND_KEY => new ConcatBaseAndKeyDeriveKeyGenerator(await this.GetSecretKey(mechanism, memorySession, p11Session), this.loggerFactory.CreateLogger<ConcatBaseAndKeyDeriveKeyGenerator>()),
            CKM.CKM_EXTRACT_KEY_FROM_KEY => this.CreateExtractKeyGenerator(mechanism),

            CKM.CKM_ECDH1_DERIVE => this.CreateEcdh1DeriveGenerator(mechanism),
            CKM.CKM_ECDH1_COFACTOR_DERIVE => this.CreateEcdh1DeriveGenerator(mechanism),

            CKM.CKM_AES_ECB_ENCRYPT_DATA => new AesDeriveKeyGenerator(CipherUtilities.GetCipher("AES/ECB/NOPADDING"), this.GetRawDataParameter(mechanism), null, this.loggerFactory.CreateLogger<AesDeriveKeyGenerator>()),
            CKM.CKM_AES_CBC_ENCRYPT_DATA => this.CreateAesCbcEncryptionGenerator(mechanism),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for derive key.")
        };
    }

    private IDeriveKeyGenerator CreateExtractKeyGenerator(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateExtractKeyGenerator.");

        try
        {
            CkP_ExtractParams extractParams = MessagePack.MessagePackSerializer.Deserialize<CkP_ExtractParams>(mechanism.MechanismParamMp);
            return new ExtractKeyFromKeyDeriveKeyGenerator((int)extractParams.Value, this.loggerFactory.CreateLogger<ExtractKeyFromKeyDeriveKeyGenerator>());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during decode CkP_ExtractParams.");
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private IDeriveKeyGenerator CreateEcdh1DeriveGenerator(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateEcdh1DeriveGenerator.");

        try
        {
            Ckp_CkEcdh1DeriveParams deriveParams = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkEcdh1DeriveParams>(mechanism.MechanismParamMp);
            Ecdh1DeriveParams ecDeriveParams = new Ecdh1DeriveParams((CKD)deriveParams.Kdf,
                deriveParams.PublicData,
                deriveParams.SharedData);

            return ((CKM)mechanism.MechanismType) switch
            {
                CKM.CKM_ECDH1_DERIVE => new Ecdh1DeriveKeyGenerator(ecDeriveParams, this.loggerFactory.CreateLogger<Ecdh1DeriveKeyGenerator>()),
                CKM.CKM_ECDH1_COFACTOR_DERIVE => new Ecdh1CofactorDeriveKeyGenerator(ecDeriveParams, this.loggerFactory.CreateLogger<Ecdh1CofactorDeriveKeyGenerator>()),
                _ => throw new InvalidParameterException($"Enum value {(CKM)mechanism.MechanismType} is not supported.")
            };
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during decode Ckp_CkEcdh1DeriveParams.");
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private AesDeriveKeyGenerator CreateAesCbcEncryptionGenerator(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateAesCbcEncryptionGenerator.");

        try
        {
            Ckp_CkAesCbcEnryptDataParams cbcEncryptData = MessagePack.MessagePackSerializer.Deserialize<Ckp_CkAesCbcEnryptDataParams>(mechanism.MechanismParamMp);

            return new AesDeriveKeyGenerator(CipherUtilities.GetCipher("AES/CBC/NOPADDING"),
                cbcEncryptData.Data,
                cbcEncryptData.Iv,
                this.loggerFactory.CreateLogger<AesDeriveKeyGenerator>());
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during decode Ckp_CkAesCbcEnryptDataParams.");
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private byte[] GetRawDataParameter(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to GetRawDataParameter.");

        try
        {
            CkP_KeyDerivationStringData rawDataParams = MessagePack.MessagePackSerializer.Deserialize<CkP_KeyDerivationStringData>(mechanism.MechanismParamMp);
            return rawDataParams.Data;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during decode CkP_RawDataParams.");
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }

    private async ValueTask<SecretKeyObject> GetSecretKey(MechanismValue mechanism, IMemorySession memorySession, IP11Session p11Session)
    {
        this.logger.LogTrace("Entering to GetSecretKey.");

        try
        {
            CkP_CkObjectHandle rawDataParams = MessagePack.MessagePackSerializer.Deserialize<CkP_CkObjectHandle>(mechanism.MechanismParamMp);
            this.logger.LogDebug("Read CkP_CkObjectHandle with handle {Handle}.", rawDataParams.Handle);

            SecretKeyObject keyObject = await this.hwServices.FindObjectByHandle<SecretKeyObject>(memorySession,
                p11Session,
                rawDataParams.Handle,
                default);

            if (!keyObject.CkaDerive)
            {
                throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, "Key object from mechanism parameter can not allowed derive.");
            }


            return keyObject;
        }
        catch (RpcPkcs11Exception)
        {
            throw;
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error during decode CkP_CkObjectHandle.");
            throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_PARAM_INVALID, $"Invalid parameter for mechanism {(CKM)mechanism.MechanismType}.", ex);
        }
    }
}