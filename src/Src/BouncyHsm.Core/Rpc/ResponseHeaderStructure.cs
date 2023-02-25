using MessagePack;

namespace BouncyHsm.Core.Rpc;

[MessagePackObject]
public class ResponseHeaderStructure
{
    [Key(0)]
    public int ReturnCode
    {
        get;
        set;
    }

    public ResponseHeaderStructure()
    {
        this.ReturnCode = 1;
    }
}