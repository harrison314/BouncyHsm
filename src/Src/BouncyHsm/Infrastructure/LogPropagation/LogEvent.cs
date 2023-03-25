namespace BouncyHsm.Infrastructure.LogPropagation;

internal struct LogEvent
{
    public DateTimeOffset Timespamt
    {
        get;
        set;
    }

    public LogLevel LogLevel
    {
        get;
        set;
    }

    public string Message
    {
        get;
        set;
    }

    public string? Tag
    {
        get;
        set;
    }

    public Exception? Exception
    {
        get;
        set;
    }
    public string Context
    {
        get;
        set;
    }
}