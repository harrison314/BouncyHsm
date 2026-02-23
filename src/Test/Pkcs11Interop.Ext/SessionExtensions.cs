using Net.Pkcs11Interop.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext;

public static class SessionExtensions
{
    public static void SessionCancel(this ISession session, IPkcs11Library library, uint ckfFlags)
    {
        if (library == null) throw new ArgumentNullException(nameof(library));
        if (session == null) throw new ArgumentNullException(nameof(session));

        ExtensionFactory.Enshure(library).SessionCancel(session, ckfFlags);
    }

    public static void EncapsulateKey(this ISession session,
        IPkcs11Library library,
        IMechanism mechanism,
        IObjectHandle publicKeyHandle,
        List<IObjectAttribute> template,
        out byte[] ciphertext,
        out IObjectHandle phKey)
    {
        if (library == null) throw new ArgumentNullException(nameof(library));
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (publicKeyHandle == null) throw new ArgumentNullException(nameof(publicKeyHandle));
        if (template == null) throw new ArgumentNullException(nameof(template));

        ExtensionFactory.Enshure(library).EncapsulateKey(session, mechanism, publicKeyHandle, template, out ciphertext, out phKey);
    }

    public static void DecapsulateKey(this ISession session,
        IPkcs11Library library,
        IMechanism mechanism,
        IObjectHandle publicKeyHandle,
        List<IObjectAttribute> template,
        byte[] ciphertext,
        out IObjectHandle phKey)
    {
        if (library == null) throw new ArgumentNullException(nameof(library));
        if (session == null) throw new ArgumentNullException(nameof(session));
        if (publicKeyHandle == null) throw new ArgumentNullException(nameof(publicKeyHandle));
        if (template == null) throw new ArgumentNullException(nameof(template));
        if (ciphertext == null) throw new ArgumentNullException(nameof(ciphertext));

        ExtensionFactory.Enshure(library).DecapsulateKey(session, mechanism, publicKeyHandle, template, ciphertext, out phKey);
    }

    public static ulong GetSessionValidationFlags(this ISession session, IPkcs11Library library, uint type)
    {
        if (library == null) throw new ArgumentNullException(nameof(library));
        if (session == null) throw new ArgumentNullException(nameof(session));

        return ExtensionFactory.Enshure(library).GetSessionValidationFlags(session, type);
    }
}
