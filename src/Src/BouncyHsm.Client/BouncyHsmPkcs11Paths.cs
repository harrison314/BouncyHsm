using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace BouncyHsm.Client;

[UnsupportedOSPlatform("browser")]
public static class BouncyHsmPkcs11Paths
{
    public const string WindowsX64 = @"runtimes\win-x64\native\BouncyHsm.Pkcs11Lib.dll";
    public const string WindowsX86 = @"runtimes\win-x86\native\BouncyHsm.Pkcs11Lib.dll";
    public const string LinuxX64 = @"runtimes/linux-x64/native/BouncyHsm.Pkcs11Lib.so";
    public const string RhelX64 = @"runtimes/linux-x64/native/BouncyHsm.Pkcs11Lib.rhel.so";

    private static Lazy<string?> linuxDistroId = new Lazy<string?>(() => GetLinuDistroId());

#if !NET8_0_OR_GREATER
    [Obsolete("For the .netstandard2.1 this property does not give correct results for RHEL like distributions.")]
#endif
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
#if NET8_0_OR_GREATER
            string? linuxId = linuxDistroId.Value;
            if (!string.IsNullOrEmpty(linuxId) && IsRhelLike(linuxId))
            {
                return RhelX64;
            }
#endif

            return LinuxX64;
        }

#if NET8_0_OR_GREATER
        throw new PlatformNotSupportedException($"Platform {RuntimeInformation.RuntimeIdentifier} is not supported by the native library.");
#else
        throw new PlatformNotSupportedException();
#endif
    }

    private static bool IsRhelLike(string rid)
    {
        return string.Equals(rid, "rhel", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rid, "almalinux", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rid, "rocky", StringComparison.OrdinalIgnoreCase)
            || string.Equals(rid, "centos", StringComparison.OrdinalIgnoreCase);
    }

    public static string? GetLinuDistroId()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return null;
        }

        if (!File.Exists("/etc/os-release"))
        {
            return null;
        }

        foreach (string line in File.ReadLines("/etc/os-release"))
        {
            if (line.StartsWith("ID="))
            {
                return line[3..].Trim('"');
            }
        }

        return null;
    }
}
