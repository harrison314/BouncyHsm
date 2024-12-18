using BouncyHsm.Core.Services.Contracts.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class PrivateKeyHelper
{
    public static bool ComputeCkaAlwaysAuthenticate(PrivateKeyObject privateKeyObject)
    {
        return privateKeyObject.CkaPrivate
                && privateKeyObject.CkaSign
                && !privateKeyObject.CkaDecrypt
                && !privateKeyObject.CkaUnwrap
                && !privateKeyObject.CkaExtractable
                && privateKeyObject.CkaSensitive;
    }
}
