using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T35_InitToken
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void InitToken_WithSoPin_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        slot.InitToken(AssemblyTestConstants.SoPin, "TestLabel1");
    }

    [TestMethod]
    public void InitToken_WithBadSoPin_Failed()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        Pkcs11Exception ex = Assert.ThrowsException<Pkcs11Exception>(() => slot.InitToken("Bad pin", "TestLabel1"));
        Assert.AreEqual(CKR.CKR_PIN_INCORRECT, ex.RV);
    }
}
