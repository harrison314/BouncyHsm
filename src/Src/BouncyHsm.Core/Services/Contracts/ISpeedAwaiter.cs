using BouncyHsm.Core.Services.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.Contracts;

public interface ISpeedAwaiter
{
    ValueTask AwaitKeyGeneration(KeyObject keyObject, DateTime utcStartTime, CancellationToken cancellationToken);

    ValueTask AwaitSignature(KeyObject keyObject, DateTime utcStartTime, CancellationToken cancellationToken);
    
    ValueTask AwaitDestroy(StorageObject keyObject, DateTime utcStartTime, CancellationToken cancellationToken);
}
