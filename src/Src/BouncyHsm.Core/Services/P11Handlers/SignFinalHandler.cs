﻿using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class SignFinalHandler : IRpcRequestHandler<SignFinalRequest, SignFinalEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<SignHandler> logger;

    public SignFinalHandler(IP11HwServices hwServices, ILoggerFactory loggerFactory, ILogger<SignHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async ValueTask<SignFinalEnvelope> Handle(SignFinalRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        DateTime utcStartTime = this.hwServices.Time.UtcNow;
        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        SignState state = p11Session.State.Ensure<SignState>();

        if (state.RequiredUserLogin && !memorySession.IsUserLogged(p11Session.SlotId))
        {
            throw new RpcPkcs11Exception(CKR.CKR_USER_NOT_LOGGED_IN, "User is not login.");
        }


        byte[] signature = state.GetSignature();

        if (request.IsSignaturePtrSet)
        {
            if (request.PullSignatureLen < (uint)signature.Length)
            {
                return new SignFinalEnvelope()
                {
                    Rv = (uint)CKR.CKR_BUFFER_TOO_SMALL,
                    Data = null
                };
            }

            StorageObject? storageObject = await this.hwServices.Persistence.TryLoadObject(p11Session.SlotId, state.PrivateKeyId, cancellationToken);
            if (storageObject == null)
            {
                storageObject = p11Session.TryLoadObject(state.PrivateKeyId);
            }

            if (storageObject is KeyObject privateKeyObject)
            {
                ISpeedAwaiter speedAwaiter = await this.hwServices.CreateSpeedAwaiter(p11Session.SlotId, this.loggerFactory, cancellationToken);
                await speedAwaiter.AwaitSignature(privateKeyObject, utcStartTime, cancellationToken);
            }
            else
            {
                this.logger.LogError("Invalid storage object loaded with id {objectId}.", state.PrivateKeyId);
                throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, "Invalid object loaded - internal error.");
            }

            p11Session.ClearState();

            return new SignFinalEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new SignatureData()
                {
                    PullSignatureLen = (uint)signature.Length,
                    Signature = signature
                }
            };
        }
        else
        {
            return new SignFinalEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new SignatureData()
                {
                    PullSignatureLen = (uint)signature.Length
                }
            };
        }
    }
}