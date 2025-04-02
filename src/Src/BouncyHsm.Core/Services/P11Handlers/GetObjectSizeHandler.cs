using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GetObjectSizeHandler : IRpcRequestHandler<GetObjectSizeRequest, GetObjectSizeEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<GetObjectSizeHandler> logger;

    public GetObjectSizeHandler(IP11HwServices hwServices, ILogger<GetObjectSizeHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<GetObjectSizeEnvelope> Handle(GetObjectSizeRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        ICryptoApiObject pkcs11Object = await this.hwServices.FindObjectByHandle(memorySession,
             p11Session,
             request.ObjectHandle,
             cancellationToken);

        uint? objectSize = pkcs11Object.TryGetSize(memorySession.IsUserLogged(p11Session.SlotId));

        return new GetObjectSizeEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = this.ConvertToSpecialUint(objectSize)
        };
    }

    private CkSpecialUint ConvertToSpecialUint(uint? size)
    {
        if (size.HasValue)
        {
            return CkSpecialUint.Create(size.Value); 
        }
        else
        {
            return CkSpecialUint.CreateInformationSensitive();
        }
    }
}