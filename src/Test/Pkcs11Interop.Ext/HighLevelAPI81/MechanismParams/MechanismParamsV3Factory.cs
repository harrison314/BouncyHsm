using Pkcs11Interop.Ext.HighLevelAPI.Factories;
using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI81.MechanismParams;

internal class MechanismParamsV3Factory : IMechanismParamsV3Factory
{
    public MechanismParamsV3Factory()
    {
    }

    public ICkChaCha20Params CreateCkChaCha20Params(uint blockCounter, byte[] nonce)
    {
        return new CkChaCha20Params(blockCounter, nonce);
    }

    public ICkChaCha20Params CreateCkChaCha20Params(ulong blockCounter, byte[] nonce)
    {
        return new CkChaCha20Params(blockCounter, nonce);
    }
}