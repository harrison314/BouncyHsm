using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class CopyObjectHandler : IRpcRequestHandler<CopyObjectRequest, CopyObjectEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<CopyObjectHandler> logger;

    public CopyObjectHandler(IP11HwServices hwServices, ILogger<CopyObjectHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async Task<CopyObjectEnvelope> Handle(CopyObjectRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} objectHandle {ObjectHandle}.",
           request.SessionId,
           request.ObjectHandle);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        if (!p11Session.IsRwSession)
        {
            throw new RpcPkcs11Exception(CKR.CKR_SESSION_READ_ONLY, "CreateObject requires readwrite session");
        }

        StorageObject originStorageObject = await this.hwServices.FindObjectByHandle<StorageObject>(memorySession, p11Session, request.ObjectHandle, cancellationToken);
        if (!originStorageObject.CkaCopyable)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ACTION_PROHIBITED, $"Object with id {originStorageObject.Id} can set CKA_COPYABLE to false.");
        }

        StorageObject storageObject = this.CloneObject(originStorageObject);
        Dictionary<CKA, IAttributeValue> dictionaryTemplate = AttrTypeUtils.BuildDictionaryTemplate(request.Template);

        try
        {
            foreach ((CKA attributeType, IAttributeValue value) in dictionaryTemplate)
            {
                this.logger.LogDebug("Updating object {objectId} with type {attributeType} value {attributeValue}.", originStorageObject.Id, attributeType, value);
                storageObject.SetValue(attributeType, value, true);
            }
        }
        catch (RpcPkcs11Exception ex) when (ex.ReturnValue == CKR.CKR_ATTRIBUTE_TYPE_INVALID)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"SetAttributeValue is inconsistent with object with id {storageObject.Id}.", ex);
        }

        storageObject.ReComputeAttributes();
        storageObject.Validate();

        uint handle = await this.hwServices.StoreObject(memorySession, p11Session, storageObject, cancellationToken);
        this.logger.LogInformation("Store new cloned object {storageObject} with Id {objectId}.", storageObject, storageObject.Id);

        return new CopyObjectEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new CopyObjectData()
            {
                ObjectHandle = handle
            }
        };
    }

    private StorageObject CloneObject(StorageObject storageObject)
    {
        this.logger.LogTrace("Entering to CloneObject with storageObject {storageObject}.", storageObject);

        StorageObjectMemento memento = storageObject.ToMemento();
        memento.Id = Guid.Empty;

        return StorageObjectFactory.CreateFromMemento(memento);
    }
}