namespace BouncyHsm.Infrastructure.LogPropagation;

public class LogDto
{
    private const string ShowSortableDateTimeFormat = "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffff";

    public string Timestamp
    {
        get;
        set;
    }

    public LogLevel Level
    {
        get;
        set;
    }

    public string Message
    {
        get;
        set;
    }

    public string? Exception
    {
        get;
        set;
    }

    public string Context
    {
        get;
        set;
    }

    public LogDto()
    {
        this.Timestamp = string.Empty;
        this.Message = string.Empty;
        this.Context = string.Empty;
    }

    internal LogDto(LogEvent logEvent)
    {

        this.Timestamp = logEvent.Timespamt.ToString("yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'ffff");
        this.Level = logEvent.LogLevel;
        this.Message = logEvent.Message;
        this.Exception = logEvent.Exception?.ToString();
        this.Context = logEvent.Context;
    }
}