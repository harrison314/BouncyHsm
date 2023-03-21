using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;

internal class WithoutRestrictionSpeedAwaiter : ISpeedAwaiter
{
    public WithoutRestrictionSpeedAwaiter()
    {

    }

    public ValueTask AwaitDestroy(StorageObject storageObject, DateTime utcStartTime, CancellationToken cancellationToken)
    {
        return new ValueTask();
    }

    public ValueTask AwaitKeyGeneration(KeyObject keyObject, DateTime utcStartTime, CancellationToken cancellationToken)
    {
        return new ValueTask();
    }

    public ValueTask AwaitSignature(KeyObject keyObject, DateTime utcStartTime, CancellationToken cancellationToken)
    {
        return new ValueTask();
    }
}
