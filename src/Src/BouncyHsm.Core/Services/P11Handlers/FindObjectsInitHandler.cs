using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using BouncyHsm.Core.Services.P11Handlers.States;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class FindObjectsInitHandler : IRpcRequestHandler<FindObjectsInitRequest, FindObjectsInitEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<FindObjectsInitHandler> logger;

    public FindObjectsInitHandler(IP11HwServices hwServices, ILogger<FindObjectsInitHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<FindObjectsInitEnvelope> Handle(FindObjectsInitRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        p11Session.State.EnsureEmpty();

        Dictionary<CKA, IAttributeValue> searchTemplate = AttrTypeUtils.BuildDictionaryTemplate(request.Template);

        IReadOnlyList<uint> hwFeatures = this.FindHwFeatures(searchTemplate);

        FindObjectSpecification findObjectSpecification = new FindObjectSpecification(searchTemplate,
            memorySession.IsUserLogged(p11Session.SlotId));

        IReadOnlyList<StorageObject> memoryObjects = p11Session.FindObjects(findObjectSpecification, cancellationToken);
        IReadOnlyList<StorageObject> persistenceObjects = await this.hwServices.Persistence.FindObjects(p11Session.SlotId, findObjectSpecification, cancellationToken);

        List<uint> handlers = new List<uint>(hwFeatures.Count + memoryObjects.Count + persistenceObjects.Count);
        handlers.AddRange(hwFeatures);

        foreach (StorageObject memoryObject in memoryObjects)
        {
            uint handle = memorySession.CreateHandle(memoryObject);
            handlers.Add(handle);
        }

        foreach (StorageObject pObject in persistenceObjects)
        {
            uint handle = memorySession.CreateHandle(pObject);
            handlers.Add(handle);
        }

        p11Session.State = new FindObjectsState(handlers);

        return new FindObjectsInitEnvelope()
        {
            Rv = (uint)CKR.CKR_OK
        };
    }

    private IReadOnlyList<uint> FindHwFeatures(IReadOnlyDictionary<CKA, IAttributeValue> searchTemplate)
    {
        List<uint> result = new List<uint>();

        ClockObject clockObject = new ClockObject(this.hwServices.Time);
        if (clockObject.IsMatch(searchTemplate))
        {
            result.Add(ClockObject.HwHandle);
        }

        return result;
    }
}
