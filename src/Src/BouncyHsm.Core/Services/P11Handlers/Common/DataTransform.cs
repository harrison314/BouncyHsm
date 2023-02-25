using BouncyHsm.Core.Rpc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class DataTransform
{
    public static CkVersion GetCurrentVersion()
    {
        Version? version = typeof(DataTransform).Assembly.GetName()?.Version;
        if (version == null)
        {
            return new CkVersion()
            {
                Major = 0,
                Minor = 0
            };
        }

        return new CkVersion()
        {
            Major = Math.Min(version.Major, 255),
            Minor = Math.Min(version.Minor, 255)
        };
    }

    public static string GetApplicationKey(AppIdentification identification)
    {
        return $"{identification.AppNonce}-{identification.Pid}";
    }
}
