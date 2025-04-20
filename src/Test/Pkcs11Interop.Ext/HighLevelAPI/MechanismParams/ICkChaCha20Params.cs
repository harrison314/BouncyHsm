using Net.Pkcs11Interop.HighLevelAPI.MechanismParams;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.HighLevelAPI.MechanismParams;

/// <summary>
/// Params for CK_CHACHA20_PARAMS for CKM_CHACHA20 mechanism
/// <code>
/// typedef struct CK_CHACHA20_PARAMS {
/// CK_BYTE_PTR pBlockCounter; pointer to block counter
/// CK_ULONG blockCounterBits; length of block counter in bits (can be either 32 or 64)
/// CK_BYTE_PTR pNonce; nonce (This should be never re-used with the same key.
/// CK_ULONG ulNonceBits; length of nonce in bits (is 64 for original, 96 for IETF and 192 for xchacha20 variant)
/// } CK_CHACHA20_PARAMS;
/// </code>
/// </summary>
public interface ICkChaCha20Params : IMechanismParams
{
    uint BlockCounter
    {
        get;
    }
}

internal class CkChaCha20ParamsGuard
{
    public static void CheckNonceBits(byte[] nonceBits)
    {
        if (nonceBits == null)
        {
            throw new ArgumentNullException(nameof(nonceBits), "Nonce bits cannot be null.");
        }

        int bits = nonceBits.Length * 8;
        if (bits != 64 && bits != 96 && bits != 192)
        {
            throw new ArgumentException("Invalid nonce bits. Valid values are 64, 96, or 192 bits (8, 12, 24B).");
        }
    }
}