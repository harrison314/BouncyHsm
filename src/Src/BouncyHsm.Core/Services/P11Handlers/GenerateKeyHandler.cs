﻿using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.Generators;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GenerateKeyHandler : IRpcRequestHandler<GenerateKeyRequest, GenerateKeyEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<GenerateKeyHandler> logger;

    public GenerateKeyHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<GenerateKeyHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async ValueTask<GenerateKeyEnvelope> Handle(GenerateKeyRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        DateTime utcStartTime = this.hwServices.Time.UtcNow;
        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        if (!memorySession.IsUserLogged(p11Session.SlotId))
        {
            throw new RpcPkcs11Exception(CKR.CKR_USER_NOT_LOGGED_IN, "CreateObject requires login");
        }

        if (!p11Session.IsRwSession)
        {
            throw new RpcPkcs11Exception(CKR.CKR_SESSION_READ_ONLY, "CreateObject requires read-write session");
        }

        Dictionary<CKA, IAttributeValue> publicKeyTemplate = AttrTypeUtils.BuildDictionaryTemplate(request.Template);

        IKeyGenerator generator = this.CreateKeyGenerator(request.Mechanism);
        generator.Init(publicKeyTemplate);
        SecretKeyObject keyObject = generator.Generate(p11Session.SecureRandom);

        keyObject.CkaLocal = true;
        keyObject.ReComputeAttributes();
        keyObject.Validate();

        ISpeedAwaiter speedAwaiter = await this.hwServices.CreateSpeedAwaiter(p11Session.SlotId, this.loggerFactory, cancellationToken);
        await speedAwaiter.AwaitKeyGeneration(keyObject, utcStartTime, cancellationToken);

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

        return new GenerateKeyEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new GenerateKeyData()
            {
                KeyHandle = handle
            }
        };
    }

    private IKeyGenerator CreateKeyGenerator(MechanismValue mechanism)
    {
        this.logger.LogTrace("Entering to CreateKeyGenerator with mechanism type {mechanismType}", (CKM)mechanism.MechanismType);

        MechanismUtils.CheckMechanism(mechanism, MechanismCkf.CKF_GENERATE);
        CKM ckMechanism = (CKM)mechanism.MechanismType;

        return ckMechanism switch
        {
            CKM.CKM_GENERIC_SECRET_KEY_GEN => new GenericSecretKeyGenerator(this.loggerFactory.CreateLogger<GenericSecretKeyGenerator>()),
            CKM.CKM_AES_KEY_GEN => new AesKeyGenerator(this.loggerFactory.CreateLogger<AesKeyGenerator>()),
            CKM.CKM_POLY1305_KEY_GEN => new Poly1305KeyGenerator(this.loggerFactory.CreateLogger<Poly1305KeyGenerator>()),
            CKM.CKM_CHACHA20_KEY_GEN => new ChaCha20KeyGenerator(this.loggerFactory.CreateLogger<ChaCha20KeyGenerator>()),
            CKM.CKM_SALSA20_KEY_GEN => new Salsa20KeyGenerator(this.loggerFactory.CreateLogger<Salsa20KeyGenerator>()),

            CKM.CKM_SHA_1_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA_1_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA224_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA224_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA256_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA256_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA384_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA384_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA512_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA512_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA512_224_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA512_224_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA512_256_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA512_256_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA512_T_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA512_T_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA3_224_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA3_224_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA3_256_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA3_256_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA3_384_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA3_384_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_SHA3_512_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_SHA3_512_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_BLAKE2B_160_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_BLAKE2B_160_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_BLAKE2B_256_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_BLAKE2B_256_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_BLAKE2B_384_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_BLAKE2B_384_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),
            CKM.CKM_BLAKE2B_512_KEY_GEN => new GenericSecretHmacKeyGenerator(CKK.CKK_BLAKE2B_512_HMAC, this.loggerFactory.CreateLogger<GenericSecretHmacKeyGenerator>()),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for generate key.")
        };
    }
}