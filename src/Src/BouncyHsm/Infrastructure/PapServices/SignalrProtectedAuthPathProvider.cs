using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using BouncyHsm.Core.Services.Contracts.P11;

namespace BouncyHsm.Infrastructure.PapServices;

public class SignalrProtectedAuthPathProvider : IProtectedAuthPathProvider
{
    private readonly IPapLoginMemoryContext context;
    private readonly ILogger<SignalrProtectedAuthPathProvider> logger;

    public SignalrProtectedAuthPathProvider(IPapLoginMemoryContext context,
        ILogger<SignalrProtectedAuthPathProvider> logger)
    {
        this.context = context;
        this.logger = logger;
    }

    public async Task<byte[]?> TryLoginProtected(ProtectedAuthPathWindowType windowType, CKU userType, SlotEntity slot, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to TryLoginProtected with windowType {windowType}, userType {userType}, slotId {slotId}.",
            windowType,
            userType,
            slot.SlotId);

        string tolkenInfo = windowType switch
        {
            ProtectedAuthPathWindowType.SetPin => $"Set PIN for: {slot.Token.Label}",
            ProtectedAuthPathWindowType.Login => $"Login to: {slot.Token.Label}",
            ProtectedAuthPathWindowType.InitPin => $"Initialize PIN for: {slot.Token.Label}",
            ProtectedAuthPathWindowType.InitToken => $"SO login for InitToken for: {slot.Token.Label}",
            _ => throw new InvalidProgramException($"Enum value {windowType} is not supported.")
        };

        byte[]? loginValue = await this.context.PerformLogin(
            userType.ToString(),
            tolkenInfo,
            cancellationToken);

        return loginValue;
    }
}