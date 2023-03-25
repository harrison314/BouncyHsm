using Serilog.Core;

namespace BouncyHsm.Infrastructure.LogPropagation;

public class SignalrLogEventSink : ILogEventSink
{
    public SignalrLogEventSink()
    {

    }

    public void Emit(Serilog.Events.LogEvent logEvent)
    {
        if (logEvent.Properties.TryGetValue("SourceContext", out Serilog.Events.LogEventPropertyValue? value))
        {
            ReadOnlySpan<char> categoryName = value.ToString().AsSpan().Trim('"');
            if (categoryName.StartsWith("BouncyHsm.Core.Services", StringComparison.Ordinal) || categoryName.StartsWith("BouncyHsm.Core.Rpc", StringComparison.Ordinal))
            {
                string message = logEvent.RenderMessage();
                string? tagValue = null;
                if (logEvent.Properties.TryGetValue("Tag", out Serilog.Events.LogEventPropertyValue? tag))
                {
                    tagValue = tag.ToString(null, null).Trim('"');
                }

                LogEvent e = new LogEvent()
                {
                    LogLevel = this.Translate(logEvent.Level),
                    Message = message,
                    Timespamt = logEvent.Timestamp,
                    Tag = tagValue,
                    Context = categoryName.ToString()
                };

                LogEventHandler.SendLog(e);
            }
        }
    }

    private LogLevel Translate(Serilog.Events.LogEventLevel level)
    {
        return level switch
        {
            Serilog.Events.LogEventLevel.Fatal => LogLevel.Critical,
            Serilog.Events.LogEventLevel.Error => LogLevel.Error,
            Serilog.Events.LogEventLevel.Information => LogLevel.Information,
            Serilog.Events.LogEventLevel.Warning => LogLevel.Warning,
            Serilog.Events.LogEventLevel.Debug => LogLevel.Debug,
            Serilog.Events.LogEventLevel.Verbose => LogLevel.Trace,
            _ => throw new InvalidProgramException($"Enum value {level} is not supported.")
        };
    }
}