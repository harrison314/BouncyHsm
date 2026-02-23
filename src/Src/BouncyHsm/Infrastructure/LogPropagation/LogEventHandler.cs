using Serilog.Events;

namespace BouncyHsm.Infrastructure.LogPropagation;

internal class LogEventHandler
{
    private static Lock syncRoot = new Lock();

    private static event EventHandler<LogEvent>? onLog;

    public static event EventHandler<LogEvent> OnLog
    {
        add
        {
            lock (syncRoot)
            {
                onLog += value;
            }
        }
        remove
        {
            lock (syncRoot)
            {
                onLog -= value;
            }
        }
    }

    internal static void SendLog(LogEvent logEvent)
    {
        onLog?.Invoke(null, logEvent);
    }
}