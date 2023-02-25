using MessagePack;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Rpc;

[MessagePackObject]
public class HeaderStructure
{
    [Key(0)]
    public string Operation
    {
        get;
        set;
    } = string.Empty;


    public HeaderStructure()
    {

    }
}
