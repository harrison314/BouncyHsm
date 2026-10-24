using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;

namespace BouncyHsm.Pkcs11IntegrationTests.IssueTests;

[TestClass]
[TestCategory(IssueTestConstants.Cathegory)]
public class T99_Issue122
{
    public TestContext? TestContext
    {
        get;
        set;
    }

    [TestMethod]
    public void GetSessionInfo_CallIssue122_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadWrite);

        Assert.AreEqual(CKS.CKS_RW_PUBLIC_SESSION, session.GetSessionInfo().State);
        session.Login(CKU.CKU_USER, AssemblyTestConstants.UserPin);
        Assert.AreEqual(CKS.CKS_RW_USER_FUNCTIONS, session.GetSessionInfo().State);
        session.Logout();
        Assert.AreEqual(CKS.CKS_RW_PUBLIC_SESSION, session.GetSessionInfo().State);
    }
}
