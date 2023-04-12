using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class CreateObjectHandler : IRpcRequestHandler<CreateObjectRequest, CreateObjectEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<CreateObjectHandler> logger;

    public CreateObjectHandler(IP11HwServices hwServices, ILogger<CreateObjectHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<CreateObjectEnvelope> Handle(CreateObjectRequest request, CancellationToken cancellationToken)
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
            throw new RpcPkcs11Exception(CKR.CKR_SESSION_READ_ONLY, "CreateObject requires readwrite session");
        }

        Dictionary<CKA, IAttributeValue> dictionaryTemplate = AttrTypeUtils.BuildDictionaryTemplate(request.Template);

        StorageObject storageObject = StorageObjectFactory.CreateEmpty(dictionaryTemplate);
        this.logger.LogDebug("Create empty storage object of type {type}.", storageObject.GetType().Name);
        this.ApplyAllAttributes(storageObject, dictionaryTemplate);
        this.ApplyReadonlyAttributes(storageObject);

        storageObject.ReComputeAttributes();
        storageObject.Validate();

        uint handle = await this.hwServices.StoreObject(memorySession,
            p11Session,
            storageObject,
            cancellationToken);

        this.logger.LogInformation("Store new object {storageObject} with Id {objectId}.", storageObject, storageObject.Id);

        return new CreateObjectEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            ObjectHandle = handle
        };
    }

    private void ApplyReadonlyAttributes(StorageObject storageObject)
    {
        this.logger.LogTrace("Entering to ApplyReadonlyAttributes with storage object {storageObject}.", storageObject);
        
        if (storageObject is KeyObject keyObject)
        {
            keyObject.CkaLocal = false;
        }

        if (storageObject is PrivateKeyObject privateKeyObject)
        {
            privateKeyObject.CkaAlwaysSensitive = false;
            privateKeyObject.CkaAlwaysAuthenticate = false;
        }
    }

    private void ApplyAllAttributes(StorageObject storageObject, Dictionary<CKA, IAttributeValue> template)
    {
        this.logger.LogTrace("Entering to ApplyAllAttributes with storage object {storageObject}.", storageObject);

        foreach ((CKA attributeType, IAttributeValue value) in template)
        {
            storageObject.SetValue(attributeType, value, false);
        }
    }
}
