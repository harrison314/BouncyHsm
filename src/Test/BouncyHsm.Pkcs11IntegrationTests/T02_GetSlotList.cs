using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;
using System.Reflection.PortableExecutable;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T02_GetSlotList
{
    [TestMethod]
    [DataRow(SlotsType.WithTokenPresent)]
    [DataRow(SlotsType.WithOrWithoutTokenPresent)]
    public void GetSlotList_Call_Success(SlotsType slotsType)
    {
        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(slotsType);

        Assert.IsNotNull(slots);
    }
}
