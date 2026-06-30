using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T10_GetSessionInfo
{
    [TestMethod]
    [DataRow(SessionType.ReadOnly, false)]
    [DataRow(SessionType.ReadWrite, true)]
    public void GetSessionInfo_Call_Success(SessionType sessionType, bool rwSession)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(sessionType);

        ISessionInfo sessionInfo = session.GetSessionInfo();

        Assert.AreEqual(rwSession, sessionInfo.SessionFlags.RwSession, "Not match flag RwSession");
        Assert.IsTrue(sessionInfo.SessionFlags.SerialSession);
        Assert.AreEqual((ulong)0, sessionInfo.DeviceError);
        Assert.AreEqual(slot.SlotId, sessionInfo.SlotId);
    }
}
