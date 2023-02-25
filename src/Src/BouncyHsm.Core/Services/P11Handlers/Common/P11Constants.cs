using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Services.P11Handlers.Common;

internal static class P11Constants
{
    public const string ManufacturerId = "BouncyHsm";
    public const string TokenModel = "BouncyHsm Token";

    public const uint MaxPinLen = 512;
    public const uint MinPinLength = 4;
}
