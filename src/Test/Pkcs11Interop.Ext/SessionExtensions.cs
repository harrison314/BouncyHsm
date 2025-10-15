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
}
