using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T03_GetSlotInfo
{
    [TestMethod]
    public void GetSlotInfo_Call_Success()
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        ISlotInfo slotInfo = slot.GetSlotInfo();

        Assert.IsNotNull(slotInfo);
        Assert.IsNotNull(slotInfo.FirmwareVersion);
        Assert.IsNotNull(slotInfo.HardwareVersion);
        Assert.IsNotNull(slotInfo.ManufacturerId);
        Assert.IsNotNull(slotInfo.SlotDescription);

        Assert.IsTrue(slotInfo.SlotFlags.TokenPresent);
    }
}
