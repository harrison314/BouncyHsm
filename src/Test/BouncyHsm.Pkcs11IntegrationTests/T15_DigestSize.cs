using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.Common;

namespace BouncyHsm.Pkcs11IntegrationTests;

[TestClass]
public class T15_DigestSize
{
    [TestMethod]
    [DataRow(32)]
    [DataRow(1024, DisplayName = "1kB")]
    [DataRow(1024 * 500, DisplayName = "500kB")]
    [DataRow(1024 * 1024, DisplayName = "1MB")]
    [DataRow(1024 * 1024 + 1024 * 512, DisplayName = "1.5MB")]
    [DataRow(1024 * 1024 * 2, DisplayName = "2MB")]
    public void Digest_LargeData_Success(int dataSize)
    {
        byte[] message = new byte[dataSize];
        Random.Shared.NextBytes(message);

        Pkcs11InteropFactories factories = new Pkcs11InteropFactories();
        using IPkcs11Library library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(factories,
            AssemblyTestConstants.P11LibPath,
            AppType.SingleThreaded);

        List<ISlot> slots = library.GetSlotList(SlotsType.WithTokenPresent);
        ISlot slot = slots.SelectTestSlot();

        using ISession session = slot.OpenSession(SessionType.ReadOnly);

        using IMechanism mechanism = factories.MechanismFactory.Create(CKM.CKM_SHA512);
        byte[] digest = session.Digest(mechanism, message);

        Assert.IsNotNull(digest);
    }
}