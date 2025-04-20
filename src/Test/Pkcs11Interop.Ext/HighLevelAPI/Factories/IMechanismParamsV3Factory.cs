using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.HighLevelAPI.Factories;

public interface IMechanismParamsV3Factory
{
    ICkChaCha20Params CreateCkChaCha20Params(uint blockCounterU32, byte[] nonce);

    ICkChaCha20Params CreateCkChaCha20Params(ulong blockCounterU64, byte[] nonce);
}
