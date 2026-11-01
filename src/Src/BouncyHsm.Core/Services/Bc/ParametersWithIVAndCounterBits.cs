using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Crypto.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace BouncyHsm.Core.Services.Bc;

public sealed class ParametersWithIVAndCounterBits : ICipherParameters
{
    public ParametersWithIV ParametersWithIV
    {
        get;
    }

    public int CounterBits
    {
        get;
    }

    public ParametersWithIVAndCounterBits(ParametersWithIV parent, int counterBits)
    {
        this.ParametersWithIV = parent;
        this.CounterBits = counterBits;
    }

    public ParametersWithIVAndCounterBits(ICipherParameters key, byte[] cb, int counterBits)
    {
        this.ParametersWithIV = new ParametersWithIV(key, cb);
        this.CounterBits = counterBits;
    }
}
