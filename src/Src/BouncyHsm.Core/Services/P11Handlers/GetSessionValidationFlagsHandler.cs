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

public partial class GetSessionValidationFlagsHandler : IRpcRequestHandler<GetSessionValidationFlagsRequest, GetSessionValidationFlagsEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<GetSessionValidationFlagsHandler> logger;

    public GetSessionValidationFlagsHandler(IP11HwServices hwServices, ILogger<GetSessionValidationFlagsHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async Task<GetSessionValidationFlagsEnvelope> Handle(GetSessionValidationFlagsRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with sessionId {SessionId}.", request.SessionId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        await memorySession.CheckIsSlotPlugged(request.SessionId, this.hwServices, cancellationToken);
        IP11Session p11Session = memorySession.EnsureSession(request.SessionId);

        if (!Enum.IsDefined<CK_SESSION_VALIDATION_FLAGS>((CK_SESSION_VALIDATION_FLAGS)request.Type))
        {
            this.logger.LogWarning("Unknown CK_SESSION_VALIDATION_FLAGS type {Type} return flags 0.", request.Type);
            return new GetSessionValidationFlagsEnvelope()
            {           
                Rv = (uint)CKR.CKR_OK,
                Data = new GetSessionValidationFlagsData()
                {
                    Flags = 0
                }
            };
        }

        this.logger.LogDebug("Return flags 0 for CK_SESSION_VALIDATION_FLAGS type {Type}.",(CK_SESSION_VALIDATION_FLAGS) request.Type);
        return new GetSessionValidationFlagsEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new GetSessionValidationFlagsData()
            {
                Flags = 0
            }
        };
    }
}
