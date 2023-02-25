namespace BouncyHsm.RpcGenerator.Schema;

public class MessageDefinition
{
    public string? Summary 
    { 
        get;
        set;
    }

    public Dictionary<string, string> Fields 
    { 
        get;
        set; 
    }

    public MessageDefinition()
    {
        this.Fields = new Dictionary<string, string>();
    }
}
