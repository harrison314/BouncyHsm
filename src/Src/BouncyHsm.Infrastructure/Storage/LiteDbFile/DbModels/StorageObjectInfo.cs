using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.LiteDbFile.DbModels;

public class StorageObjectInfo
{
    public Guid Id
    {
        get;
        set;
    }

    public uint SlotId
    {
        get;
        set;
    }

    public ulong LabelHash
    {
        get;
        set;
    }

    public bool IsPrivate
    {
        get;
        set;
    }


    public ulong? IdHash
    {
        get;
        set;
    }

    public uint CkaClass
    {
        get;
        set;
    }

    public uint? KeyType
    {
        get;
        set;
    }

    public uint? CertType
    {
        get;
        set;
    }

    public DateTime Created
    {
        get;
        set;
    }

    public StorageObjectInfo()
    {

    }
}
