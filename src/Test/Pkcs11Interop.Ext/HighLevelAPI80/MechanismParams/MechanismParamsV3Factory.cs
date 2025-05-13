using Pkcs11Interop.Ext.HighLevelAPI.Factories;
using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI80.MechanismParams;

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

    public ICkSalsa20Params CreateCkSalsa20Params(ulong blockCounter, byte[] nonce)
    {
        return new CkSalsa20Params(blockCounter, nonce);
    }

    public ICkEddsaParams CreateCkEddsaParams(bool phFlag, byte[]? contextData)
    {
        return new CkEddsaParams(phFlag, contextData);
    }
}