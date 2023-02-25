using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T10_GetSessionInfo
{
    [TestMethod]
    public void GetSessionInfo_Call_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);

        ISessionInfo sessionInfo = session.GetSessionInfo();

        Assert.IsFalse(sessionInfo.SessionFlags.RwSession);
        Assert.IsTrue(sessionInfo.SessionFlags.SerialSession);
        Assert.AreEqual((ulong)0, sessionInfo.DeviceError);
        Assert.AreEqual(slot.SlotId, sessionInfo.SlotId);
    }
}
