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

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
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

        uint handle = await this.hwServices.StoreObject(memorySession,
            p11Session,
            keyObject,
            cancellationToken);

        if (this.logger.IsEnabled(LogLevel.Information))
        {
            this.logger.LogInformation("Create new symetric key using {generator}. Key <Id: {publicKeyId}, CK_ID: {publicKeyCkId}, CK_LABEL: {publicKeyCkLabel}>",
                generator.ToString(),
                keyObject.Id,
                BitConverter.ToString(keyObject.CkaId),
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
            CKM.CKM_GENERIC_SECRET_KEY_GEN => new GenericSeecretKeyGenerator(this.loggerFactory.CreateLogger<GenericSeecretKeyGenerator>()),
            CKM.CKM_AES_KEY_GEN => new AesKeyGenerator(this.loggerFactory.CreateLogger<AesKeyGenerator>()),

            _ => throw new RpcPkcs11Exception(CKR.CKR_MECHANISM_INVALID, $"Invalid mechanism {ckMechanism} for generate key.")
        };
    }
}