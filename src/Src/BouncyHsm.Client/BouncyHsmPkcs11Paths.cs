using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace BouncyHsm.Client;

[UnsupportedOSPlatform("browser")]
public static class BouncyHsmPkcs11Paths
{
    public const string WindowsX64 = @"runtimes\win-x64\native\BouncyHsm.Pkcs11Lib.dll";
    public const string WindowsX86 = @"runtimes\win-x86\native\BouncyHsm.Pkcs11Lib.dll";
    public const string LinuxX64 = @"runtimes/linux-x64/native/BouncyHsm.Pkcs11Lib.so";

    public static string CurrentPlatformSpecificPkcs11Path
    {
        get => GetPlatformPkcs11Path();
    }

    private static string GetPlatformPkcs11Path()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            if (Environment.Is64BitProcess)
            {
                return WindowsX64;
            }
            else
            {
                return WindowsX86;
            }

        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux) && Environment.Is64BitProcess)
        {
            return LinuxX64;
        }

#if NET8_0_OR_GREATER
        throw new PlatformNotSupportedException($"Platfrom {RuntimeInformation.RuntimeIdentifier} is not supported by the native library.");
#else
        throw new PlatformNotSupportedException();
#endif
    }
}

