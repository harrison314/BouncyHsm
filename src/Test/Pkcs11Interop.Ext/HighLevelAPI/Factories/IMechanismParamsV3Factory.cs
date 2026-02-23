using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
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

    ICkSalsa20ChaCha20Polly1305Params CreateCkSalsa20ChaCha20Polly1305Params(byte[] nonce, byte[]? aadData);

    ICkSalsa20Params CreateCkSalsa20Params(ulong blockCounter, byte[] nonce);

    ICkEddsaParams CreateCkEddsaParams(bool phFlag, byte[]? contextData);

    ICkSignAdditionalContextParams CreateSignAdditionalContextParams(ulong hedgeVariant, byte[]? context);

    ICkHashSignAdditionalContextParams CreateCkHashSignAdditionalContextParams(ulong hedgeVariant, byte[]? context, CKM hash);

    ICkHkdfParams CreateCkHkdfParams(bool extract,
        bool expand,
        CKM hashMechanism,
        uint saltType,
        IObjectHandle? saltKey,
        byte[]? salt,
        byte[]? info);

}
