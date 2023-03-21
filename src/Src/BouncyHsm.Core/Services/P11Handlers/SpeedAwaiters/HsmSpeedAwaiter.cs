using BouncyHsm.Core.Services.Contracts;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;

internal class HsmSpeedAwaiter : BaseSpeedAwaiter
{
    public HsmSpeedAwaiter(ITimeAccessor timeAccessor, ILogger<HsmSpeedAwaiter> logger) 
        : base(timeAccessor, logger)
    {
    }

    protected override double[] GetMultiplicationVector()
    {
        return new double[] { 2.3, 4.0, 1.2 };
    }
}
