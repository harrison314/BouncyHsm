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

    public DateTime StartAt
    {
        get;
        set;
    }

    public DateTime LastInteraction 
    { 
        get; 
        set;
    }

    public ApplicationSessionDto()
    {
        this.ApplicationName = string.Empty;
        this.ComputerName = string.Empty;
    }
}
