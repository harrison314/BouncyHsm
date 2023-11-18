using System.Runtime.InteropServices;

namespace BouncyHsm.Pkcs11IntegrationTests;

internal static class AssemblyTestConstants
{
    public static string P11LibPath
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                return "BouncyHsm.Pkcs11Lib.dll";
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return "./BouncyHsm.Pkcs11Lib-x64.so";
            }

            throw new PlatformNotSupportedException();
        }
    }

    public const string UserPin = "123456";

    public const string SoPin = "12345678";
}
