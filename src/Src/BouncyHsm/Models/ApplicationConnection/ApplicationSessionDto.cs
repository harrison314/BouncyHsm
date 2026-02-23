namespace BouncyHsm.Models.ApplicationConnection;

[SmartAnalyzers.CSharpExtensions.Annotations.TwinType(typeof(BouncyHsm.Core.UseCases.Contracts.ApplicationSession))]
public class ApplicationSessionDto
{
    public Guid ApplicationSessionId
    {
        get;
        set;
    }

    public string ComputerName
    {
        get;
        set;
    }

    public string ApplicationName 
    { 
        get; 
        set;
    }

    public uint Pid
    {
        get;
        set;
    }

    public string[] CmdArguments
    {
        get;
        set;
    }

    public DateTimeOffset StartAt
    {
        get;
        set;
    }

    public DateTimeOffset LastInteraction 
    { 
        get; 
        set;
    }

    public ApplicationSessionDto()
    {
        this.ApplicationName = string.Empty;
        this.ComputerName = string.Empty;
        this.CmdArguments = Array.Empty<string>();
    }
}
