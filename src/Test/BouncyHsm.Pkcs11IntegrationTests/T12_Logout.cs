using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T12_Logout
{
    [TestMethod]
    public void Logout_Call_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);
        session.Logout();
    }

    [TestMethod]
    public void Logout_CallMultipleSessions_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session1 = slot.OpenSession(SessionType.ReadOnly);
        session1.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        using ISession session2 = slot.OpenSession(SessionType.ReadOnly);
        Assert.AreEqual(CKS.CKS_RO_USER_FUNCTIONS, session1.GetSessionInfo().State);
        Assert.AreEqual(CKS.CKS_RO_USER_FUNCTIONS, session2.GetSessionInfo().State);

        session2.Logout();

        Assert.AreEqual(CKS.CKS_RO_PUBLIC_SESSION, session1.GetSessionInfo().State);
        Assert.AreEqual(CKS.CKS_RO_PUBLIC_SESSION, session2.GetSessionInfo().State);
    }

    [TestMethod]
    public void Logout_CallMultipleSessions2_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session1 = slot.OpenSession(SessionType.ReadOnly);
        using ISession session2 = slot.OpenSession(SessionType.ReadOnly);
        Assert.AreEqual(CKS.CKS_RO_PUBLIC_SESSION, session1.GetSessionInfo().State);
        Assert.AreEqual(CKS.CKS_RO_PUBLIC_SESSION, session2.GetSessionInfo().State);

        session1.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);

        Assert.AreEqual(CKS.CKS_RO_USER_FUNCTIONS, session1.GetSessionInfo().State);
        Assert.AreEqual(CKS.CKS_RO_USER_FUNCTIONS, session2.GetSessionInfo().State);

        session2.Logout();

        Assert.AreEqual(CKS.CKS_RO_PUBLIC_SESSION, session1.GetSessionInfo().State);
        Assert.AreEqual(CKS.CKS_RO_PUBLIC_SESSION, session2.GetSessionInfo().State);
    }
}
