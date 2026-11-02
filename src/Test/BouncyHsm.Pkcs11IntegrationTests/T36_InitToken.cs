using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T36_InitToken
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void InitPIN_WithPin_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        slot.InitToken(AssemblyTestConstants.SoPin, "TestLabel1");

        using ISession session = slot.OpenSession(SessionType.ReadWrite);
        session.Login(CKU.CKU_SO, AssemblyTestConstants.SoPin);
        session.InitPin("Foobar");
        session.InitPin(AssemblyTestConstants.UserPin);
    }

    [TestMethod]
    public void InitPIN_ReadonlySession_Failed()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        slot.InitToken(AssemblyTestConstants.SoPin, "TestLabel1");

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_SO, AssemblyTestConstants.SoPin);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => session.InitPin(AssemblyTestConstants.UserPin));
        Assert.AreEqual(CKR.CKR_SESSION_READ_ONLY, ex.RV);
    }

    [TestMethod]
    public void InitPIN_NotLoggedIn_Failed()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        slot.InitToken(AssemblyTestConstants.SoPin, "TestLabel1");

        using ISession session = slot.OpenSession(SessionType.ReadWrite);

        Pkcs11Exception ex = Assert.Throws<Pkcs11Exception>(() => session.InitPin(AssemblyTestConstants.UserPin));
        Assert.AreEqual(CKR.CKR_USER_NOT_LOGGED_IN, ex.RV);
    }
}