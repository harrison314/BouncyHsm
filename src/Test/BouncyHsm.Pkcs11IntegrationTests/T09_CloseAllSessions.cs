using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T09_CloseAllSessions
{
    [TestMethod]
    public void CloseAllSessions_Call_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        ISession session = slot.OpenSession(SessionType.ReadOnly);
        ISession session2 = slot.OpenSession(SessionType.ReadOnly);
        ISession session3 = slot.OpenSession(SessionType.ReadOnly);

        slot.CloseAllSessions();
    }
}
