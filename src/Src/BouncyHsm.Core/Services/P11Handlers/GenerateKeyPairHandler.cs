using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Crypto.Signers;
using System.Runtime.CompilerServices;
using System.Threading;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GenerateKeyPairHandler : IRpcRequestHandler<GenerateKeyPairRequest, GenerateKeyPairEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<GenerateKeyPairHandler> logger;

    public GenerateKeyPairHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<GenerateKeyPairHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async ValueTask<GenerateKeyPairEnvelope> Handle(GenerateKeyPairRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        DateTime utcStartTime = this.hwServices.Time.UtcNow;
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

        Dictionary<CKA, IAttributeValue> publicKeyTemplate = AttrTypeUtils.BuildDictionaryTemplate(request.PublicKeyTemplate);
        Dictionary<CKA, IAttributeValue> privateKeyTemplate = AttrTypeUtils.BuildDictionaryTemplate(request.PrivateKeyTemplate);

        IKeyPairGenerator generator = this.CreateKeyPairGenerator(request.Mechanism);
        generator.Init(publicKeyTemplate, privateKeyTemplate);
        (PublicKeyObject publicKeyObject, PrivateKeyObject privateKeyObject) = generator.Generate(p11Session.SecureRandom);

        await this.UpdatePrivateKey(p11Session, privateKeyObject, cancellationToken);
        this.UpdatePublicObject(publicKeyObject);

        publicKeyObject.ReComputeAttributes();
        privateKeyObject.ReComputeAttributes();

        publicKeyObject.Validate();
        privateKeyObject.Validate();

        ISpeedAwaiter speedAwaiter = await this.hwServices.CreateSpeedAwaiter(p11Session.SlotId, this.loggerFactory, cancellationToken);
        await speedAwaiter.AwaitKeyGeneration(privateKeyObject, utcStartTime, cancellationToken);

        uint publicKeyHandle = await this.hwServices.StoreObject(memorySession, p11Session, publicKeyObject, cancellationToken);
        uint privateKeyHandle = await this.hwServices.StoreObject(memorySession, p11Session, privateKeyObject, cancellationToken);

        if (this.logger.IsEnabled(LogLevel.Information))
        {
            this.logger.LogInformation("Create new key pair using {generator}. Public key <Id: {publicKeyId}, CK_ID: {publicKeyCkId}, CK_LABEL: {publicKeyCkLabel}>, private key: <Id: {publicKeyId}, CK_ID: {privateKeyCkId}, CK_LABEL: {privateKeyCkLabel}>",
                generator.ToString(),
                publicKeyObject.Id,
                Convert.ToHexString(publicKeyObject.CkaId),
                publicKeyObject.CkaLabel,
                privateKeyObject.Id,
                Convert.ToHexString(privateKeyObject.CkaId),
                privateKeyObject.CkaLabel);
        }

        return new GenerateKeyPairEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new GenerateKeyPairData()
            {
                PrivateKeyHandle = privateKeyHandle,
                PublicKeyHandle = publicKeyHandle
            }
        };
    }

    private IKeyPairGenerator CreateKeyPairGenerator(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateKeyPairGenerator with mechanism type {mechanismType}", (CKM)mechanism.MechanismType);

        MechanismUtils.CheckMechanism(mechanism, MechanismCkf.CKF_GENERATE_KEY_PAIR);
        CKM ckMechanism = (CKM)mechanism.MechanismType;

        return ckMechanism switch
        {
            CKM.CKM_RSA_PKCS_KEY_PAIR_GEN => new RsaKeyPairGenerator(false, this.loggerFactory.CreateLogger<RsaKeyPairGenerator>()),
            CKM.CKM_RSA_X9_31_KEY_PAIR_GEN => new RsaKeyPairGenerator(true, this.loggerFactory.CreateLogger<RsaKeyPairGenerator>()),
            CKM.CKM_ECDSA_KEY_PAIR_GEN => new EcdsaKeyPairGenerator(this.loggerFactory.CreateLogger<EcdsaKeyPairGenerator>()),
            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for generate key pair.")
        };
    }

    private async ValueTask UpdatePrivateKey(IP11Session p11Session, PrivateKeyObject privateKeyObject, CancellationToken cancellationToken)
    {
        privateKeyObject.CkaLocal = true;
        privateKeyObject.CkaAlwaysSensitive = privateKeyObject.CkaSensitive;

        SlotEntity? slot = await this.hwServices.Persistence.GetSlot(p11Session.SlotId, cancellationToken);
        if (slot == null)
        {
            throw new InvalidOperationException("Invalid slotId.");
        }

        if (slot.Token.SimulateQualifiedArea)
        {
            if (PrivateKeyHelper.ComputeCkaAlwaysAuthenticate(privateKeyObject))
            {
                privateKeyObject.CkaAlwaysAuthenticate = true;
                this.logger.LogInformation("Private key mark as AlwaysAuthenticate.");
            }
            else
            {
                privateKeyObject.CkaAlwaysAuthenticate = false;
            }
        }
    }

    private void UpdatePublicObject(PublicKeyObject publicKeyObject)
    {
        publicKeyObject.CkaLocal = true;
    }
}