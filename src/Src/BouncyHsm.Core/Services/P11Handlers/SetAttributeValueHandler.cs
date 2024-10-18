using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class SetAttributeValueHandler : IRpcRequestHandler<SetAttributeValueRequest, SetAttributeValueEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<SetAttributeValueHandler> logger;

    public SetAttributeValueHandler(IP11HwServices hwServices, ILogger<SetAttributeValueHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<SetAttributeValueEnvelope> Handle(SetAttributeValueRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId} objectHandle {ObjectHandle}.",
            request.SessionId,
            request.ObjectHandle);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPluuged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        if (!p11Session.IsRwSession)
        {
            throw new RpcPkcs11Exception(CKR.CKR_SESSION_READ_ONLY, "CreateObject requires readwrite session");
        }

        StorageObject storageObject = await this.hwServices.FindObjectByHandle<StorageObject>(memorySession, p11Session, request.ObjectHandle, cancellationToken);
        if (!storageObject.CkaModifiable)
        {
            throw new RpcPkcs11Exception(CKR.CKR_ACTION_PROHIBITED, $"Object with id {storageObject.Id} can set CKA_MODIFIABLE to false.");
        }

        bool storeOnToken = storageObject.CkaToken;

        Dictionary<CKA, IAttributeValue> dictionaryTemplate = AttrTypeUtils.BuildDictionaryTemplate(request.Template);

        try
        {
            foreach ((CKA attributeType, IAttributeValue value) in dictionaryTemplate)
            {
                this.logger.LogDebug("Updating object {objectId} with type {attributeType} value {attributeValue}.", storageObject.Id, attributeType, value);
                storageObject.SetValue(attributeType, value, true);
            }
        }
        catch (RpcPkcs11Exception ex) when (ex.ReturnValue == CKR.CKR_ATTRIBUTE_TYPE_INVALID)
        {
            throw new RpcPkcs11Exception(CKR.CKR_TEMPLATE_INCONSISTENT, $"SetAttributeValue is inconsistent with object with id {storageObject.Id}.", ex);
        }

        if (storeOnToken != storageObject.CkaToken)
        {
            throw new RpcPkcs11Exception(CKR.CKR_GENERAL_ERROR, "Attribute CKA_TOKEN is not modifiable in BouncyHsm.");
        }

        storageObject.ReComputeAttributes();
        storageObject.Validate();

        if (storageObject.CkaToken)
        {
            await this.hwServices.Persistence.UpdateObject(p11Session.SlotId, storageObject, cancellationToken);
            this.logger.LogInformation("Update object with id {objectId} on token.", storageObject.Id);
        }
        else
        {
            p11Session.UpdateObject(storageObject);
            this.logger.LogInformation("Update object with id {objectId} in session {sessionId}.", storageObject.Id, p11Session.SessionId);
        }

        return new SetAttributeValueEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }
}
