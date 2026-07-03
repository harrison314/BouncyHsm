using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class DestroyObjectHandler : IRpcRequestHandler<DestroyObjectRequest, DestroyObjectEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILoggerFactory loggerFactory;
    private readonly ILogger<DestroyObjectHandler> logger;

    public DestroyObjectHandler(IP11HwServices hwServices,
        ILoggerFactory loggerFactory,
        ILogger<DestroyObjectHandler> logger)
    {
        this.hwServices = hwServices;
        this.loggerFactory = loggerFactory;
        this.logger = logger;
    }

    public async Task<DestroyObjectEnvelope> Handle(DestroyObjectRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        DateTimeOffset utcStartTime = this.hwServices.Time.GetUtcNow();
        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        StorageObject storageObject = await this.hwServices.FindObjectByHandle<StorageObject>(memorySession,
            p11Session,
            request.ObjectHandle,
            cancellationToken);

        if (!storageObject.CkaDestroyable)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ACTION_PROHIBITED, $"Object with id {storageObject.Id} is not destroyable.");
        }

        if (!memorySession.IsUserLogged(p11Session.SlotId))
        {
            if (storageObject.CkaPrivate)
            {
                throw new RpcPkcs11Exception(CKR.CKR_USER_NOT_LOGGED_IN, "A logged in user is required to work with private objects (CKA_PRIVATE = true).");
            }

            if (storageObject.CkaToken)
            {
                throw new RpcPkcs11Exception(CKR.CKR_USER_NOT_LOGGED_IN, "A logged in user is required to write to the token (CKA_TOKEN = true).");
            }
        }

        if (storageObject.CkaToken)
        {
            if (!p11Session.IsRwSession)
            {
                throw new RpcPkcs11Exception(CKR.CKR_SESSION_READ_ONLY, "A read-write session is required to destroy object from the token.");
            }

            memorySession.DestroyObjectHandle(storageObject.Id);
            await this.hwServices.Persistence.DestroyObject(p11Session.SlotId, storageObject, cancellationToken);

            ISpeedAwaiter speedAwaiter = await this.hwServices.CreateSpeedAwaiter(p11Session.SlotId, this.loggerFactory, cancellationToken);
            await speedAwaiter.AwaitDestroy(storageObject, utcStartTime, cancellationToken);
        }
        else
        {
            if (storageObject.CkaPrivate)
            {
                throw new RpcPkcs11Exception(CKR.CKR_USER_NOT_LOGGED_IN, "A logged in user is required to work with private objects (CKA_PRIVATE = true).");
            }

            memorySession.DestroyObjectHandle(storageObject.Id);
            p11Session.DestroyObject(storageObject);
        }

        this.logger.LogInformation("Destroy object <Id: {objectKeyId}, CK_LABEL: {objectKeyCkLabel}>.",
            storageObject.Id,
            storageObject.CkaLabel);

        return new DestroyObjectEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}