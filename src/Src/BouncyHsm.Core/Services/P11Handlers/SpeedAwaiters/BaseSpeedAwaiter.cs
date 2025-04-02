using BouncyHsm.Core.Services.Contracts;
using BouncyHsm.Core.Services.Contracts.Entities;
using Microsoft.Extensions.Logging;

namespace BouncyHsm.Core.Services.P11Handlers.SpeedAwaiters;

internal abstract class BaseSpeedAwaiter : ISpeedAwaiter
{
    private readonly ITimeAccessor timeAccessor;
    private readonly ILogger logger;

    public BaseSpeedAwaiter(ITimeAccessor timeAccessor, ILogger logger)
    {
        this.timeAccessor = timeAccessor;
        this.logger = logger;
    }

    public async ValueTask AwaitKeyGeneration(KeyObject keyObject, DateTime utcStartTime, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to AwaitKeyGeneration with storageObject {storageObject}.", keyObject);

        TimeSpan elapsedTime = this.timeAccessor.UtcNow - utcStartTime;
        TimeSpan waitTime = keyObject.Accept(new GenerateKeyVisitor(this.GetMultiplicationVector()));

        await this.TryWait(waitTime - elapsedTime, cancellationToken);
    }

    public async ValueTask AwaitSignature(KeyObject keyObject, DateTime utcStartTime, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to AwaitSignature with storageObject {storageObject}.", keyObject);

        TimeSpan elapsedTime = this.timeAccessor.UtcNow - utcStartTime;
        TimeSpan waitTime = keyObject.Accept(new SignVisitor(this.GetMultiplicationVector()));

        await this.TryWait(waitTime - elapsedTime, cancellationToken);
    }

    public async ValueTask AwaitDestroy(StorageObject storageObject, DateTime utcStartTime, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to AwaitDestroy with storageObject {storageObject}.", storageObject);

        const int blockSize = 8 * 1024;

        TimeSpan elapsedTime = this.timeAccessor.UtcNow - utcStartTime;
        uint objectSize = storageObject.TryGetSize(true) ?? blockSize;
        objectSize += blockSize - 1;

        int blocks = (int)objectSize / blockSize;

        double[] multiplicator = this.GetMultiplicationVector();
        double result = blocks * 20.0 * this.GetMultiplicator(multiplicator, 1);

        await this.TryWait(TimeSpan.FromMilliseconds(result) - elapsedTime, cancellationToken);
    }

    protected abstract double[] GetMultiplicationVector();

    private async ValueTask TryWait(TimeSpan waitTime, CancellationToken cancellationToken)
    {
        this.logger.LogTrace("Entering to TryWait with {waitTime}.", waitTime);
        if (waitTime > TimeSpan.Zero)
        {
            if (waitTime > TimeSpan.FromMinutes(5.0))
            {
                waitTime = TimeSpan.FromMinutes(5.0);
            }

            this.logger.LogDebug("Waiting {waitTime} for slowing operation.", waitTime);
            await Task.Delay(waitTime, cancellationToken);
        }
    }

    private double GetMultiplicator(double[] mi, int pi)
    {
        if (pi < mi.Length)
        {
            return mi[pi];
        }

        return 1.0;
    }
}
