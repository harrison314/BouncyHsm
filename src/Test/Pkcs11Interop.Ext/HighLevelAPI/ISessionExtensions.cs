using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext.HighLevelAPI;

internal interface ISessionExtensions
{
    void SessionCancel(ISession session, uint CkfFlags);

    void EncapsulateKey(ISession session,
        IMechanism mechanism,
        IObjectHandle publicKeyHandle,
        List<IObjectAttribute> template,
        out byte[] ciphertext,
        out IObjectHandle phKey);

    void DecapsulateKey(ISession session,
        IMechanism mechanism,
        IObjectHandle publicKeyHandle,
        List<IObjectAttribute> template,
        byte[] ciphertext,
        out IObjectHandle phKey);

    ulong GetSessionValidationFlags(ISession session, uint type);
}
