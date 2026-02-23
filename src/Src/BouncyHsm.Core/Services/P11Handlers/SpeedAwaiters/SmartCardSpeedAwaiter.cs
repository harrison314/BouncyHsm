using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;

internal class SmartCardSpeedAwaiter : BaseSpeedAwaiter
{
    public SmartCardSpeedAwaiter(TimeProvider timeProvider, ILogger<SmartCardSpeedAwaiter> logger)
        : base(timeProvider, logger)
    {
    }

    protected override double[] GetMultiplicationVector()
    {
        return new double[] { 12.5, 18.0, 18.0 };
    }
}