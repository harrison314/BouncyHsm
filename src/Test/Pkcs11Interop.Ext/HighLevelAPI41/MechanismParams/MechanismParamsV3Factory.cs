using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext.HighLevelAPI.Factories;
using Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

namespace Pkcs11Interop.Ext.HighLevelAPI41.MechanismParams;

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
    public ICkSignAdditionalContextParams CreateSignAdditionalContextParams(ulong hedgeVariant, byte[]? context)
    {
        return new CkSignAdditionalContextParams((uint)hedgeVariant, context);
    }

    public ICkHashSignAdditionalContextParams CreateCkHashSignAdditionalContextParams(ulong hedgeVariant, byte[]? context, CKM hash)
    {
        return new CkHashSignAdditionalContextParams((uint)hedgeVariant, context, (uint)hash);
    }

    public ICkHkdfParams CreateCkHkdfParams(bool extract,
       bool expand,
       CKM hashMechanism,
       uint saltType,
       IObjectHandle? saltKey,
       byte[]? salt,
       byte[]? info)
    {
        return new CkHkdfParams(extract, expand, hashMechanism, saltType, saltKey, salt, info);
    }
}
