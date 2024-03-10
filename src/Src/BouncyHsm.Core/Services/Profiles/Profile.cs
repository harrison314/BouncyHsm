namespace BouncyHsm.Core.Services.Profiles;

public class Profile
{
    public string Name
    { 
        get; 
        set; 
    }

    public string? Description
    { 
        get;
        set; 
    }

    public string? Author
    {
        get;
        set;
    }

    public List<ProfileOperation> Operations
    {
        get;
        set;
    }

    public Profile()
    {
        this.Name = string.Empty;
        this.Operations = new List<ProfileOperation>();
    }
}
