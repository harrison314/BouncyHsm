using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Pkcs11Interop.Ext.HighLevelAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Pkcs11Interop.Ext;

internal static class ExtensionFactory
{
    private static ConditionalWeakTable<IPkcs11Library, ISessionExtensions> cache = new ConditionalWeakTable<IPkcs11Library, ISessionExtensions>();

    public static ISessionExtensions Enshure(IPkcs11Library library)
    {
        if (cache.TryGetValue(library, out ISessionExtensions? extensions))
        {
            return extensions;
        }
        else
        {
            extensions = Create(library);
            cache.Add(library, extensions);
            return extensions;
        }
    }

    private static ISessionExtensions Create(IPkcs11Library library)
    {
        System.Reflection.FieldInfo? pkcs11FiledInfo = library.GetType().GetField("_pkcs11Library", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (pkcs11FiledInfo == null)
        {
            throw new InvalidOperationException("Cannot get _pkcs11Library field info");
        }

        object? pkcs11 = pkcs11FiledInfo.GetValue(library);
        if (pkcs11 == null)
        {
            throw new InvalidOperationException("Cannot get _pkcs11Library value");
        }

        System.Reflection.FieldInfo? libHandleFieldInfo = pkcs11.GetType().GetField("_libraryHandle", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (libHandleFieldInfo == null)
        {
            throw new InvalidOperationException("Cannot get _libraryHandle field info");
        }

        object? libHandle = libHandleFieldInfo.GetValue(library);
        if (libHandle == null)
        {
            throw new InvalidOperationException("Cannot get _libraryHandle value");
        }

        IntPtr nativeLibHandle = (IntPtr)libHandle;

        if (Platform.NativeULongSize == 4)
        {
            if (Platform.StructPackingSize == 0)
            {
                return new Pkcs11Interop.Ext.HighLevelAPI40.SessionExtensions(nativeLibHandle);
            }
            else
            {
                return new Pkcs11Interop.Ext.HighLevelAPI41.SessionExtensions(nativeLibHandle);
            }
        }
        else
        {
            if (Platform.StructPackingSize == 0)
            {
                return new Pkcs11Interop.Ext.HighLevelAPI80.SessionExtensions(nativeLibHandle);

            }
            else
            {
                return new Pkcs11Interop.Ext.HighLevelAPI81.SessionExtensions(nativeLibHandle);
            }
        }
    }
}
