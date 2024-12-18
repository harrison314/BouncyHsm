using BouncyHsm.Client;
using System.Runtime.InteropServices;

namespace BouncyHsm.Pkcs11ScenarioTests;

internal static class BchClient
{
    private static HttpClient httpClient = new HttpClient();

    private const string BouncyhsmEndpoint = "https://localhost:7007/";

    public static IBouncyHsmClient Client
    {
        get => new BouncyHsmClient(BouncyhsmEndpoint, httpClient);
    }

    public static string P11LibPath
    {
        get
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                if (Environment.Is64BitProcess)
                {
                    return @"BouncyHsm.Pkcs11Lib.dll";
                }
                else
                {
                    return @"BouncyHsm.Pkcs11Lib.dll";
                }

            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                return @"BouncyHsm.Pkcs11Lib-x64.so";
            }

            throw new PlatformNotSupportedException();
        }
    }
}