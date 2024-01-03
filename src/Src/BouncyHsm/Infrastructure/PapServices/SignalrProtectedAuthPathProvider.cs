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

    public async Task<byte[]?> TryLoginProtected(CKU userType, SlotEntity slot, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to TryLoginProtected.");

        string tolkenInfo = $"Login to: {slot.Token.Label}";
        byte[]? loginValue = await this.context.PerformLogin(
            userType.ToString(),
            tolkenInfo,
            cancellationToken);

        return loginValue;
    }
}