using BouncyHsm.Core.Services.Contracts.P11;
using Org.BouncyCastle.Crypto;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Contracts.Generators.PrfDataParams;

internal interface IPrfDataParam
{
    CK_PRF_DATA_TYPE Type
    {
        get;
    }

    void Apply(IMac prfFunction, ref PrfDataContext context);
}
