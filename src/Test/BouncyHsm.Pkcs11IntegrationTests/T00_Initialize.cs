using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T00_Initialize
{
    [DataTestMethod]
    [DataRow(AppType.SingleThreaded)]
    [DataRow(AppType.MultiThreaded)]
    public async Task InitializeAndFinalize_Call_Success(AppType apptype)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            apptype);

        await Task.Delay(10);

        library.Dispose();
    }
}
