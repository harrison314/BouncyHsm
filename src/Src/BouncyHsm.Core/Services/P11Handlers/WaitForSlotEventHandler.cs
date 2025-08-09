using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class WaitForSlotEventHandler : IRpcRequestHandler<WaitForSlotEventRequest, WaitForSlotEventEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<WaitForSlotEventHandler> logger;

    public WaitForSlotEventHandler(IP11HwServices hwServices, ILogger<WaitForSlotEventHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public Task<WaitForSlotEventEnvelope> Handle(WaitForSlotEventRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle.");

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);

        if (request.Flags != (uint)CKF.CKF_DONT_BLOCK)
        {
            this.logger.LogError("WaitForSlotEvent accept flag only CKF_DONT_BLOCK actual value is {flags}. Returns CKR_ARGUMENTS_BAD.", request.Flags);
            return Task.FromResult(new WaitForSlotEventEnvelope()
            {
                Rv = (uint)CKR.CKR_ARGUMENTS_BAD
            });
        }

        if (!request.IsSlotPtrSet)
        {
            this.logger.LogError("In WaitForSlotEvent is pSlot NULL. Returns CKR_ARGUMENTS_BAD.");
            return Task.FromResult(new WaitForSlotEventEnvelope()
            {
                Rv = (uint)CKR.CKR_ARGUMENTS_BAD
            });
        }

        if (request.IsReservedPtrSet)
        {
            this.logger.LogError("In WaitForSlotEvent is not pReseved NULL. Returns CKR_ARGUMENTS_BAD.");
            return Task.FromResult(new WaitForSlotEventEnvelope()
            {
                Rv = (uint)CKR.CKR_ARGUMENTS_BAD
            });
        }

        uint? changedSlotId = memorySession.GetLastSlotEvent();
        if (changedSlotId.HasValue)
        {
            return Task.FromResult(new WaitForSlotEventEnvelope()
            {
                Rv = (uint)CKR.CKR_OK,
                Data = new WaitForSlotEventData()
                {
                    SlotId = changedSlotId.Value
                }
            });
        }
        else
        {
            return Task.FromResult(new WaitForSlotEventEnvelope()
            {
                Rv = (uint)CKR.CKR_NO_EVENT
            });
        }
    }
}
