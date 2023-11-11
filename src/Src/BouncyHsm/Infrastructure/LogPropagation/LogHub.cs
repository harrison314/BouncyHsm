using Microsoft.AspNetCore.SignalR;
using Serilog.Events;
using System.Runtime.CompilerServices;
using System.Threading.Channels;

namespace BouncyHsm.Infrastructure.LogPropagation;

public class LogHub : Hub<ILogHubClient>
{
    private readonly ILogger<LogHub> logger;

    public LogHub(ILogger<LogHub> logger)
    {
        this.logger = logger;
    }

#pragma warning disable CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
    public IAsyncEnumerable<LogDto> GetLogStream(string? tagFilter, LogLevel minLogLevel, [EnumeratorCancellation] CancellationToken cancellationToken)
#pragma warning restore CS8424 // The EnumeratorCancellationAttribute will have no effect. The attribute is only effective on a parameter of type CancellationToken in an async-iterator method returning IAsyncEnumerable
    {
        this.logger.LogTrace("Entering to GetLogStream with tagFilter {tagFilter}, minLogLevel {minLogLevel}.", tagFilter, minLogLevel);

        try
        {
            Channel<LogDto> buffer = Channel.CreateUnbounded<LogDto>(new UnboundedChannelOptions()
            {
                AllowSynchronousContinuations = true,
            });

            EventHandler<LogEvent> handler = (_, log) =>
            {
                if (!string.IsNullOrEmpty(tagFilter)
                && (log.Tag == null || !log.Tag.StartsWith(tagFilter, StringComparison.OrdinalIgnoreCase)))
                {
                    return;
                }

                if (minLogLevel > log.LogLevel)
                {
                    return;
                }

                _ = buffer.Writer.TryWrite(new LogDto(log));
            };

            LogEventHandler.OnLog += handler;
            _ = cancellationToken.Register(new Action(() =>
            {
                LogEventHandler.OnLog -= handler;
                buffer.Writer.Complete();
                this.logger.LogDebug("GetLogStream has canceled.");
            }));

            return buffer.Reader.ReadAllAsync();
        }
        catch (Exception ex)
        {
            this.logger.LogError(ex, "Error in GetLogStream.");
            throw;
        }
    }
}
