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
    private readonly ILogger<DestroyObjectHandler> logger;

    public DestroyObjectHandler(IP11HwServices hwServices, ILogger<DestroyObjectHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<DestroyObjectEnvelope> Handle(DestroyObjectRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);


        if (!memorySession.IsUserLogged(p11Session.SlotId))
        {
            throw new RpcPkcs11Exception(CKR.CKR_USER_NOT_LOGGED_IN, "DestroyObject requires login");
        }

        if (!p11Session.IsRwSession)
        {
            throw new RpcPkcs11Exception(CKR.CKR_SESSION_READ_ONLY, "DestroyObject requires read-write session");
        }

        StorageObject storageObject = await this.hwServices.FindObjectByHandle<StorageObject>(memorySession,
            p11Session,
            request.ObjectHandle,
            cancellationToken);

        if (!storageObject.CkaDestroyable)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ACTION_PROHIBITED, $"Object with id {storageObject.Id} is not destroyable.");
        }

        if (storageObject.CkaToken)
        {
            memorySession.DestroyObjectHandle(storageObject.Id);
            await this.hwServices.Persistence.DestroyObject(p11Session.SlotId, storageObject, cancellationToken);
        }
        else
        {
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