namespace BouncyHsm.RpcGenerator.Schema;

public class RpcMethodDefinition
{
    public string Request 
    { 
        get; 
        set; 
    }

    public string Response 
    { 
        get; 
        set; 
    }

    public string? Summary
    { 
        get;
        set;
    }

    public RpcMethodDefinition()
    {
        this.Request = string.Empty;
        this.Response = string.Empty;
    }
}
