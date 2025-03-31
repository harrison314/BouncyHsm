using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace BouncyHsm.Client;

[UnsupportedOSPlatform("browser")]
public static class BouncyHsmPkcs11Paths
{
    public const string WindowsX64 = @"runtimes\win-x64\native\BouncyHsm.Pkcs11Lib.dll";
    public const string WindowsX86 = @"runtimes\win-x86\native\BouncyHsm.Pkcs11Lib.dll";
    public const string LinuxX64 = @"runtimes/linux-x64/native/BouncyHsm.Pkcs11Lib.so";
    public const string RhelX64 = @"runtimes/rhel-x64/native/BouncyHsm.Pkcs11Lib.so";

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
            string rid = RuntimeInformation.RuntimeIdentifier;
            if (IsRhelLike(rid))
            {
                return RhelX64;
            }
#endif

            return LinuxX64;
        }

#if NET8_0_OR_GREATER
        throw new PlatformNotSupportedException($"Platfrom {RuntimeInformation.RuntimeIdentifier} is not supported by the native library.");
#else
        throw new PlatformNotSupportedException();
#endif
    }

    private static bool IsRuntimeIdentifierPrefix(string rid, string prefix)
    {
        return string.Equals(prefix, rid, StringComparison.OrdinalIgnoreCase)
            || rid.StartsWith(string.Concat(prefix, "."), StringComparison.OrdinalIgnoreCase)
            || rid.StartsWith(string.Concat(prefix, "-"), StringComparison.OrdinalIgnoreCase);
    }

    // See: https://github.com/dotnet/runtime/blob/main/src/libraries/Microsoft.NETCore.Platforms/src/runtime.json
    private static bool IsRhelLike(string rid)
    {
        return IsRuntimeIdentifierPrefix(rid, "rhel")
            || IsRuntimeIdentifierPrefix(rid, "rocky")
            || IsRuntimeIdentifierPrefix(rid, "ol")
            || IsRuntimeIdentifierPrefix(rid, "miraclelinux")
            || IsRuntimeIdentifierPrefix(rid, "centos");
    }
}
