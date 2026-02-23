using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;

internal class HsmSpeedAwaiter : BaseSpeedAwaiter
{
    public HsmSpeedAwaiter(TimeProvider timeProvider, ILogger<HsmSpeedAwaiter> logger) 
        : base(timeProvider, logger)
    {
    }

    protected override double[] GetMultiplicationVector()
    {
        return new double[] { 1.2, 2.0, 0.6 };
    }
}
