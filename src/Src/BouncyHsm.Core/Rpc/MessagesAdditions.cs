using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BouncyHsm.Core.Rpc;

public partial class CkSpecialUint
{
    internal static CkSpecialUint CreateUnavailableInformation()
    {
        return new CkSpecialUint()
        {
            UnavailableInformation = true
        };
    }

    internal static CkSpecialUint CreateEffectivelyInfinite()
    {
        return new CkSpecialUint()
        {
            EffectivelyInfinite = true,
        };
    }

    internal static CkSpecialUint CreateInformationSensitive()
    {
        return new CkSpecialUint()
        {
            InformationSensitive = true
        };
    }

    internal static CkSpecialUint Create(uint value)
    {
        return new CkSpecialUint()
        {
            Value = value
        };
    }
}
