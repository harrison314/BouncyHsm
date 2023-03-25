namespace BouncyHsm.Spa.Client.Services.Client;

public class LogDto
{
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
}
