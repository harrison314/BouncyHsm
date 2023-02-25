using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T04_GetTokenInfo
{
    [TestMethod]
    public void GetTokenInfo_Call_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        ITokenInfo tokenInfo = slot.GetTokenInfo();

        Assert.IsNotNull(tokenInfo);
        Assert.IsNotNull(tokenInfo.FirmwareVersion);
        Assert.IsNotNull(tokenInfo.HardwareVersion);
        Assert.IsNotNull(tokenInfo.ManufacturerId);
        Assert.IsNotNull(tokenInfo.Model);
        Assert.IsNotNull(tokenInfo.ManufacturerId);
        Assert.IsNotNull(tokenInfo.SerialNumber);

        Assert.IsTrue(tokenInfo.TokenFlags.UserPinInitialized);
        Assert.IsTrue(tokenInfo.TokenFlags.LoginRequired);
        Assert.IsTrue(tokenInfo.TokenFlags.TokenInitialized);
    }
}
