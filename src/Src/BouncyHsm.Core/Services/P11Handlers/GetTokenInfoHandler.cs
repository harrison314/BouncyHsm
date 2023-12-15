using BouncyHsm.Core.Rpc;
using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;
using BouncyHsm.Core.Services.P11Handlers.Common;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers;

public partial class GetTokenInfoHandler : IRpcRequestHandler<GetTokenInfoRequest, GetTokenInfoEnvelope>
{
    private readonly IP11HwServices hwServices;
    private readonly ILogger<GetTokenInfoHandler> logger;

    public GetTokenInfoHandler(IP11HwServices hwServices, ILogger<GetTokenInfoHandler> logger)
    {
        this.hwServices = hwServices;
        this.logger = logger;
    }

    public async ValueTask<GetTokenInfoEnvelope> Handle(GetTokenInfoRequest request, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to Handle with req SlotId {SlotId}.",
            request.SlotId);

        IMemorySession memorySession = this.hwServices.ClientAppCtx.EnsureMemorySession(request.AppId);
        SlotEntity? slot = await this.hwServices.Persistence.GetSlot(request.SlotId, cancellationToken);

        if (slot == null)
        {
            this.logger.LogDebug("SlotId {SlotId} not found.", request.SlotId);
            return new GetTokenInfoEnvelope()
            {
                Rv = (uint)CKR.CKR_SLOT_ID_INVALID,
                Data = null
            };
        }

        if (slot.Token == null)
        {
            this.logger.LogDebug("Not found token for slotId {SlotId}.", request.SlotId);
            return new GetTokenInfoEnvelope()
            {
                Rv = (uint)CKR.CKR_TOKEN_NOT_PRESENT,
                Data = null
            };
        }

        CkVersion currentVersion = DataTransform.GetCurrentVersion();
        CkSpecialUint unknown = new CkSpecialUint()
        {
            UnavailableInformation = true,
            EffectivelyInfinite = false,
            InformationSensitive = false,
            Value = 0
        };

        CkSpecialUint infinite = new CkSpecialUint()
        {
            UnavailableInformation = false,
            EffectivelyInfinite = true,
            InformationSensitive = false,
            Value = 0
        };

        MemorySessionStatus info = memorySession.GetStatus();
        uint flags = this.BuildFlags(slot.Token);

        return new GetTokenInfoEnvelope()
        {
            Rv = (uint)CKR.CKR_OK,
            Data = new Rpc.TokenInfo()
            {
                Label = slot.Token.Label,
                ManufacturerId = P11Constants.ManufacturerId,
                Model = P11Constants.TokenModel,
                SerialNumber = slot.Token.SerialNumber,
                Flags = flags,
                MaxSessionCount = infinite,
                SessionCount = new CkSpecialUint()
                {
                    UnavailableInformation = false,
                    EffectivelyInfinite = false,
                    Value = (uint)(info.RwSessionCount + info.RoSessionCount)
                },
                MaxRwSessionCount = infinite,
                RwSessionCount = new CkSpecialUint()
                {
                    UnavailableInformation = false,
                    EffectivelyInfinite = false,
                    InformationSensitive = false,
                    Value = (uint)(info.RwSessionCount)
                },
                MaxPinLen = P11Constants.MaxPinLen,
                MinPinLen = P11Constants.MinPinLength,
                TotalPublicMemory = unknown,
                FreePublicMemory = unknown,
                TotalPrivateMemory = unknown,
                FreePrivateMemory = unknown,
                HardwareVersion = currentVersion,
                FirmwareVersion = currentVersion,
                UtcTime = string.Format("{0:yyyyMMddHHmmss}00", this.hwServices.Time.UtcNow)
            }
        };
    }

    private uint BuildFlags(Contracts.Entities.TokenInfo token)
    {
        uint flags = CKF.CKF_CLOCK_ON_TOKEN | CKF.CKF_LOGIN_REQUIRED | CKF.CKF_USER_PIN_INITIALIZED | CKF.CKF_TOKEN_INITIALIZED;
        if (token.SimulateHwRng)
        {
            flags |= CKF.CKF_RNG;
        }

        if (token.SimulateQualifiedArea)
        {
            flags |= CKF.CKF_SECONDARY_AUTHENTICATION;
        }

        if (token.IsUserPinLocked)
        {
            flags |= CKF.CKF_USER_PIN_LOCKED;
        }

        if (token.IsSoPinLocked)
        {
            flags |= CKF.CKF_SO_PIN_LOCKED;
        }

        if (token.SimulateProtectedAuthPath)
        {
            flags |= CKF.CKF_PROTECTED_AUTHENTICATION_PATH;
        }

        return flags;
    }
}