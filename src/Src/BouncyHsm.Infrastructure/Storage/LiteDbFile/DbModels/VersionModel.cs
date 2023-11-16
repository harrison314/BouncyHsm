using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Infrastructure.Storage.LiteDbFile.DbModels;

public class VersionModel
{
    public string Id
    {
        get;
        set;
    }

    public DateTime MigrationTime
    {
        get;
        set;
    }

    public VersionModel()
    {
        this.Id = string.Empty;
    }
}
