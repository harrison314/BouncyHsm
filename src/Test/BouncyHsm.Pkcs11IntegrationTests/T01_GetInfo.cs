using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T01_GetInfo
{
    [TestMethod]
    public void InitializeAndFinalize_Call_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        ILibraryInfo info = library.GetInfo();

        Assert.IsNotNull(info.ManufacturerId);
        Assert.IsNotNull(info.LibraryVersion);
        Assert.IsNotNull(info.CryptokiVersion);
    }
}
