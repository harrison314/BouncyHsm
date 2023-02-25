using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace BouncyHsm.RpcGenerator.Schema;

public class RpcDefinition
{
    public string Name
    {
        get;
        set;
    }

    public string Description
    {
        get;
        set;
    }

    public Dictionary<string, RpcMethodDefinition> Rpc
    {
        get;
        set;
    }


    public Dictionary<string, MessageDefinition> Messages
    {
        get;
        set;
    }

    public RpcDefinition()
    {
        this.Name = string.Empty;
        this.Description = string.Empty;
        this.Rpc = new Dictionary<string, RpcMethodDefinition>();
        this.Messages = new Dictionary<string, MessageDefinition>();
    }

    public static RpcDefinition Load(string path)
    {
        string content = System.IO.File.ReadAllText(path);
        IDeserializer deserializer = new DeserializerBuilder()
           .Build();

        return deserializer.Deserialize<RpcDefinition>(content);
    }
}
