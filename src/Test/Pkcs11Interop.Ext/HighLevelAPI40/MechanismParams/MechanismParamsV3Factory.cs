using Pkcs11Interop.Ext.HighLevelAPI.Factories;
using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.HighLevelAPI40.MechanismParams;

internal class MechanismParamsV3Factory : IMechanismParamsV3Factory
{
    public ICkChaCha20Params CreateCkChaCha20Params(uint blockCounter, byte[] nonce)
    {
        return new CkChaCha20Params(blockCounter, nonce);
    }

    public ICkChaCha20Params CreateCkChaCha20Params(ulong blockCounter, byte[] nonce)
    {
        return new CkChaCha20Params(blockCounter, nonce);
    }

    public ICkSalsa20ChaCha20Polly1305Params CreateCkSalsa20ChaCha20Polly1305Params(byte[] nonce, byte[]? aadData)
    {
        return new CkSalsa20ChaCha20Polly1305Params(nonce, aadData);
    }
}
